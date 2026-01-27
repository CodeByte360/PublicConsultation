using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PublicConsultation.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PublicConsultation.Infrastructure.Services;

public class ConsultationStatusHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ConsultationStatusHostedService> _logger;

    public ConsultationStatusHostedService(IServiceProvider serviceProvider, ILogger<ConsultationStatusHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Consultation Status Service running.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await UpdateConsultationStatusesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating consultation statuses.");
            }
        }
    }

    private async Task UpdateConsultationStatusesAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.Now;

            // Publish Drafts that have reached their StartDate
            var draftsToPublish = dbContext.DraftDocuments
                .Where(d => d.Status == "Draft" && d.ConsultationStartDate <= now)
                .ToList();

            if (draftsToPublish.Any())
            {
                foreach (var draft in draftsToPublish)
                {
                    draft.Status = "Published";
                    _logger.LogInformation($"Auto-publishing draft: {draft.Title} (ID: {draft.Oid})");
                }
            }

            // Close Published documents that have passed their EndDate
            // "Citizen not see published draft after ConsultationEndDate"
            // We can change status to "Closed" to strictly enforce this.
            var publishedToClose = dbContext.DraftDocuments
                .Where(d => d.Status == "Published" && d.ConsultationEndDate < now)
                .ToList();

            if (publishedToClose.Any())
            {
                foreach (var doc in publishedToClose)
                {
                    doc.Status = "Closed";
                    _logger.LogInformation($"Auto-closing consultation: {doc.Title} (ID: {doc.Oid})");
                }
            }

            if (draftsToPublish.Any() || publishedToClose.Any())
            {
                await dbContext.SaveChangesAsync(stoppingToken);
            }
        }
    }
}

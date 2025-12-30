using Microsoft.Extensions.Configuration;
using PublicConsultation.Core.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PublicConsultation.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpSettings = _configuration.GetSection("SmtpSettings");
        var host = smtpSettings["Host"];
        var port = int.Parse(smtpSettings["Port"] ?? "587");
        var username = smtpSettings["Username"];
        var password = smtpSettings["Password"];
        var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = enableSsl
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(username!, "Public Consultation System"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        await client.SendMailAsync(mailMessage);
    }

    public async Task SendBulkEmailAsync(List<string> tos, string subject, string body)
    {
        // Simple implementation: Loop and send
        // In a real high-volume system, we would use a specialized provider or a background queue
        foreach (var to in tos)
        {
            try
            {
                await SendEmailAsync(to, subject, body);
            }
            catch
            {
                // Log and continue to next user
            }
        }
    }
}

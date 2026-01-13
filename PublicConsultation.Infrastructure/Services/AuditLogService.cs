using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;

namespace PublicConsultation.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _dbContext;

    public AuditLogService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogActivityAsync(string activity, string details, string userEmail, string ipAddress, Guid? documentId = null)
    {
        try
        {
            var lastLog = await _dbContext.AuditLogs
                .OrderByDescending(l => l.CreatedDate)
                .FirstOrDefaultAsync();

            var previousHash = lastLog?.CurrentHash ?? "GENESIS_BLOCK";

            var log = new AuditLog
            {
                Activity = activity ?? "Unknown",
                Details = details ?? string.Empty,
                UserEmail = userEmail ?? "Anonymous",
                IpAddress = ipAddress ?? "System",
                RelatedDocumentId = documentId,
                PreviousHash = previousHash,
                CreatedDate = DateTime.UtcNow
            };

            log.CurrentHash = CalculateHash(log);

            _dbContext.AuditLogs.Add(log);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AuditLog Error: {ex.Message}");
        }
    }

    public async Task<List<AuditLog>> GetLogsAsync(Guid? documentId = null)
    {
        var query = _dbContext.AuditLogs.AsQueryable();
        if (documentId.HasValue)
        {
            query = query.Where(l => l.RelatedDocumentId == documentId.Value);
        }

        return await query
            .Include(l => l.RelatedDocument)
            .OrderByDescending(l => l.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> VerifyIntegrityAsync()
    {
        try
        {
            var logs = await _dbContext.AuditLogs.OrderBy(l => l.CreatedDate).ToListAsync();
            string expectedPreviousHash = "GENESIS_BLOCK";

            foreach (var log in logs)
            {
                if (log.PreviousHash != expectedPreviousHash) return false;

                var recalculatedHash = CalculateHash(log);
                if (log.CurrentHash != recalculatedHash) return false;

                expectedPreviousHash = log.CurrentHash;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Integrity Verification Error: {ex.Message}");
            return false;
        }
    }

    private string CalculateHash(AuditLog log)
    {
        var input = $"{log.Activity}|{log.Details}|{log.UserEmail}|{log.IpAddress}|{log.RelatedDocumentId}|{log.PreviousHash}|{log.CreatedDate:yyyyMMddHHmmss}";
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}

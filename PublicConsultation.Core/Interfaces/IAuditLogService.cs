using System;
using System.Threading.Tasks;
using PublicConsultation.Core.Entities;

namespace PublicConsultation.Core.Interfaces;

public interface IAuditLogService
{
    Task LogActivityAsync(string activity, string details, string userEmail, string ipAddress, Guid? documentId = null);
    Task<List<AuditLog>> GetLogsAsync(Guid? documentId = null);
    Task<bool> VerifyIntegrityAsync();
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace PublicConsultation.Core.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendBulkEmailAsync(List<string> tos, string subject, string body);
}

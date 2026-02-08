using System.Threading.Tasks;

namespace PublicConsultation.Core.Interfaces;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string key);
    Task<bool> ValidateOtpAsync(string key, string otp);
}

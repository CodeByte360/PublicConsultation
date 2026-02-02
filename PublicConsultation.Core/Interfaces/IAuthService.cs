using PublicConsultation.Core.Entities;
using System.Threading.Tasks;

namespace PublicConsultation.Core.Interfaces;

public interface IAuthService
{
    Task<UserAccount> RegisterUserAsync(UserAccount user, string password, Guid? roleId = null);
    Task<UserAccount?> LoginAsync(string email, string password);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    Task<bool> UserExistsAsync(string email);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null);
    Task<bool> IsPhoneUniqueAsync(string phone, Guid? excludeUserId = null);
    Task<bool> IsNidUniqueAsync(long nid, Guid? excludeUserId = null);
    Task LogoutAsync();
}

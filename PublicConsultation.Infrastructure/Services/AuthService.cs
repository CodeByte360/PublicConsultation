using Microsoft.EntityFrameworkCore;
using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;
using BCrypt.Net;

namespace PublicConsultation.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public async Task<UserAccount?> LoginAsync(string email, string password)
    {
        var user = await _context.UserAccounts
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null) return null;

        if (VerifyPassword(password, user.PasswordHash))
        {
            return user;
        }

        return null;
    }

    public async Task<UserAccount> RegisterUserAsync(UserAccount user, string password, Guid? roleId = null)
    {
        // Pre-validation for uniqueness
        if (!await IsEmailUniqueAsync(user.Email))
            throw new Exception("Email address is already in use.");

        if (!await IsPhoneUniqueAsync(user.PhoneNumber))
            throw new Exception("Phone number is already in use.");

        if (!await IsNidUniqueAsync(user.NIDNumber))
            throw new Exception("NID number is already in use.");

        user.PasswordHash = HashPassword(password);

        if (roleId.HasValue)
        {
            user.RoleId = roleId.Value;
        }
        else
        {
            // Assign default "Citizen" role
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Citizen");
            if (defaultRole != null)
            {
                user.RoleId = defaultRole.Oid;
            }
            else
            {
                throw new Exception("Default role 'Citizen' not found.");
            }
        }

        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _context.UserAccounts.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null)
    {
        return !await _context.UserAccounts.AnyAsync(u => u.Email == email && u.Oid != excludeUserId);
    }

    public async Task<bool> IsPhoneUniqueAsync(string phone, Guid? excludeUserId = null)
    {
        return !await _context.UserAccounts.AnyAsync(u => u.PhoneNumber == phone && u.Oid != excludeUserId);
    }

    public async Task<bool> IsNidUniqueAsync(long nid, Guid? excludeUserId = null)
    {
        return !await _context.UserAccounts.AnyAsync(u => u.NIDNumber == nid && u.Oid != excludeUserId);
    }

    public Task LogoutAsync()
    {
        // For cookie auth or server-side session, this might do more.
        // For now, it's a placeholder if state is handled client-side.
        return Task.CompletedTask;
    }
}

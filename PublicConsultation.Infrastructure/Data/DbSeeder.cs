using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PublicConsultation.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IAuthService authService)
    {
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { Name = "Admin", Description = "System Administrator" },
                new Role { Name = "Official", Description = "Government Official" },
                new Role { Name = "Citizen", Description = "General Citizen" }
            };

            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
        }

        var adminUser = await context.UserAccounts.Include(u => u.Role).FirstOrDefaultAsync(u => u.Username == "admin");
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

        if (adminRole != null)
        {
            if (adminUser == null)
            {
                adminUser = new UserAccount
                {
                    Username = "admin",
                    Email = "admin@consultation.gov.bd",
                    FullNameEnglish = "System Administrator",
                    IsVerified = true,
                    RoleId = adminRole.Oid
                };
                await authService.RegisterUserAsync(adminUser, "Admin@123", adminRole.Oid);
            }
            else if (adminUser.Role?.Name != "Admin")
            {
                adminUser.RoleId = adminRole.Oid;
                await context.SaveChangesAsync();
            }
        }
    }
}

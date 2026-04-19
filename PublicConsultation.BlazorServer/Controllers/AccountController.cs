using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PublicConsultation.Core.Interfaces;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace PublicConsultation.BlazorServer.Controllers;

[Route("account")]
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password, [FromForm] string returnUrl = "/")
    {
        try
        {
            var user = await _authService.LoginAsync(email, password);

            if (user == null)
            {
                return Redirect($"/login?error=Invalid credentials");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "Citizen"),
                new Claim("UserId", user.Oid.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return LocalRedirect(returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login for user {Email}", email);
            return Redirect($"/login?error=An internal error occurred. Please try again later.");
        }
    }

    [HttpPost("login-biometric")]
    public async Task<IActionResult> LoginBiometric([FromForm] string email, [FromForm] string returnUrl = "/")
    {
        try
        {
            var biometric = await _authService.GetBiometricDataAsync(email);
            if (biometric == null)
            {
                _logger.LogWarning("Unauthorized biometric login attempt for email: {Email}", email);
                return Redirect($"/login?error=Biometric login is only available for registered Admins and Officers.");
            }

            var context = HttpContext.RequestServices.GetRequiredService<PublicConsultation.Infrastructure.Data.ApplicationDbContext>();
            var user = await context.UserAccounts.Include(u => u.Role).FirstOrDefaultAsync(u => u.Oid == biometric.UserAccountId);

            if (user == null || !user.IsActive)
            {
                return Redirect($"/login?error=User account is inactive or not found.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "Citizen"),
                new Claim("UserId", user.Oid.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddMinutes(120) // Extended session for biometric login
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("Successful biometric login for user {Email}", email);
            return LocalRedirect(returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during biometric login for user {Email}", email);
            return Redirect($"/login?error=An internal error occurred. Please try again later.");
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}

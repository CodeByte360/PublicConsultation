using System.ComponentModel.DataAnnotations;

namespace PublicConsultation.Core.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required!")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required!")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm Password is required!")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full Name (English) is required!")]
    public string FullNameEnglish { get; set; } = string.Empty;

    public string? FullNameBangla { get; set; }

    [Required(ErrorMessage = "Mobile Number is required!")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "NID Number is required!")]
    public long NIDNumber { get; set; }

    public string? Designation { get; set; }

    public string? Address { get; set; }

    public Guid? PoliceStationId { get; set; }

    public string? ProfilePictureUrl { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PublicConsultation.Core.Entities;

public class UserAccount : BaseEntity
{
    [Key]
    public Guid Oid { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "Email Is Required!")]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(15)]
    [Required(ErrorMessage = "PhoneNumber Is Required!")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(100)]
    [Required(ErrorMessage = "Full Name (English) Is Required!")]
    public string? FullNameEnglish { get; set; } = string.Empty;

    public string? FullNameBangla { get; set; } = string.Empty;

    [StringLength (20)]
    [Required(ErrorMessage = "NID Number Is Required!")]
    public long NIDNumber { get; set; }

    public string? Designation { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid? PoliceStationId { get; set; }
    [ForeignKey("PoliceStationId")]
    public virtual PoliceStation? PoliceStation { get; set; }

    // Foreign Key
    public Guid RoleId { get; set; }
    [ForeignKey("RoleId")]
    public virtual Role? Role { get; set; }
}

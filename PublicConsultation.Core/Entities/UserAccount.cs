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
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string? FullNameEnglish { get; set; }

    public string? FullNameBangla { get; set; }

    [StringLength(17)]
    [Required(ErrorMessage = "NID Number Is Required!")]
    public string? NIDNumber { get; set; }

    public string? Designation { get; set; }

    public string? Address { get; set; }

    public string? ProfilePictureUrl { get; set; }
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

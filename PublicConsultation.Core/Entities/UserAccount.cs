using System;
using System.ComponentModel.DataAnnotations;

namespace PublicConsultation.Core.Entities;

public class UserAccount : BaseEntity
{

    [Key]
    public Guid Oid { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? FullNameEnglish { get; set; }
    public string? FullNameBangla { get; set; }
    public string? NIDNumber { get; set; }
    public string? Designation { get; set; }
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? Division { get; set; }
    public bool IsVerified { get; set; }

    // Foreign Key
    public Guid RoleId { get; set; }
    public virtual  Role? Role { get; set; }
}

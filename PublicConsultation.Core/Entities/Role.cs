using System;
using System.ComponentModel.DataAnnotations;

namespace PublicConsultation.Core.Entities;

public class Role : BaseEntity
{

    [Key]
    public Guid Oid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

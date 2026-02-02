using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PublicConsultation.Core.Entities;

public class Division : BaseEntity
{
    [Key]
    public Guid Oid { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<District> Districts { get; set; } = new List<District>();
}

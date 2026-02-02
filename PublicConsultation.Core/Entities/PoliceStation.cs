using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PublicConsultation.Core.Entities;

public class PoliceStation : BaseEntity
{
    [Key]
    public Guid Oid { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public Guid DistrictId { get; set; }

    [ForeignKey("DistrictId")]
    public virtual District? District { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace PublicConsultation.Core.Entities;

public class District : BaseEntity
{
    [Key]
    public Guid Oid { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public Guid DivisionId { get; set; }

    [ForeignKey("DivisionId")]
    public virtual Division? Division { get; set; }

    public virtual ICollection<PoliceStation> PoliceStations { get; set; } = new List<PoliceStation>();
}

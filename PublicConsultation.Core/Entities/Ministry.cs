using System.ComponentModel.DataAnnotations;

namespace PublicConsultation.Core.Entities;

public class Ministry : BaseEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

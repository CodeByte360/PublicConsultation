using System;

namespace PublicConsultation.Core.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

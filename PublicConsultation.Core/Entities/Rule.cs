using System;
using System.ComponentModel.DataAnnotations;

namespace PublicConsultation.Core.Entities;

public class Rule : BaseEntity
{

    [Key]
    public Guid Oid { get; set; }
    public Guid DraftDocumentId { get; set; }
    public DraftDocument? DraftDocument { get; set; }

    public string RuleNumber { get; set; } = string.Empty;
    public string SectionTitle { get; set; } = string.Empty; // Renamed from Title to avoid confusion
    public string ExistingProvision { get; set; } = string.Empty; // Renamed for clarity
    public string ProposedProvision { get; set; } = string.Empty; // Renamed for clarity
    public int DisplayOrder { get; set; }
}

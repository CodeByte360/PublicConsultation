#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace PublicConsultation.Core.Entities;

public class AuditLog : BaseEntity
{
    [Key]
    public Guid Oid { get; set; }
    public string Activity { get; set; } = string.Empty; // e.g., "Document Published", "Draft Updated"
    public string Details { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;

    // Blockchain-lite: Chaining previous hash to ensure integrity
    public string PreviousHash { get; set; } = string.Empty;
    public string CurrentHash { get; set; } = string.Empty;

    public Guid? RelatedDocumentId { get; set; }
    public DraftDocument? RelatedDocument { get; set; }
}

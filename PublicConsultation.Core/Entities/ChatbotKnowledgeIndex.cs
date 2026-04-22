using System;
using System.ComponentModel.DataAnnotations;

namespace PublicConsultation.Core.Entities;

/// <summary>
/// Stores auto-learned keyword-to-entity mappings built by the background training service.
/// This index is rebuilt daily from database content and past conversations.
/// </summary>
public class ChatbotKnowledgeIndex : BaseEntity
{
    [Key]
    public Guid Oid { get; set; }

    /// <summary>
    /// The learned keyword or phrase (lowercased).
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// The intent category this keyword maps to (e.g., DocumentQuery, RuleQuery).
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Intent { get; set; } = string.Empty;

    /// <summary>
    /// Source entity type where this keyword was extracted from.
    /// </summary>
    [StringLength(50)]
    public string SourceEntity { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the source entity for direct linking.
    /// </summary>
    public Guid? SourceEntityId { get; set; }

    /// <summary>
    /// TF-IDF or frequency-based weight (higher = more important).
    /// </summary>
    public double Weight { get; set; }

    /// <summary>
    /// How many times this keyword was matched in user questions.
    /// Increases over time as users ask questions.
    /// </summary>
    public int HitCount { get; set; }

    /// <summary>
    /// Last time this entry was refreshed by the training service.
    /// </summary>
    public DateTime LastTrainedDate { get; set; } = DateTime.UtcNow;
}

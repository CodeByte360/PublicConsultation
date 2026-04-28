using System;
using System.ComponentModel.DataAnnotations;

namespace PublicConsultation.Core.Entities;

/// <summary>
/// Persists chatbot conversation messages for analytics and history tracking.
/// </summary>
public class ChatbotConversation : BaseEntity
{
    [Key]
    public Guid Oid { get; set; }

    /// <summary>
    /// The user who asked the question (nullable for anonymous/unauthenticated users).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Groups messages belonging to the same conversation session.
    /// </summary>
    [Required]
    [StringLength(64)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The user's original question text.
    /// </summary>
    [Required]
    public string UserMessage { get; set; } = string.Empty;

    /// <summary>
    /// The chatbot's generated response.
    /// </summary>
    [Required]
    public string BotResponse { get; set; } = string.Empty;

    /// <summary>
    /// The detected intent category (e.g., DocumentQuery, RuleQuery, StatisticsQuery).
    /// </summary>
    [StringLength(50)]
    public string Intent { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score of the intent match (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; set; }
}

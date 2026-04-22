using System;
using System.Collections.Generic;

namespace PublicConsultation.Core.DTOs;

/// <summary>
/// Represents a user's question sent to the chatbot.
/// </summary>
public class ChatbotQuestionDto
{
    /// <summary>
    /// The user's natural language question.
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// Session identifier to group messages in a conversation.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Optional authenticated user ID.
    /// </summary>
    public Guid? UserId { get; set; }
}

/// <summary>
/// Represents the chatbot's response to a question.
/// </summary>
public class ChatbotAnswerDto
{
    /// <summary>
    /// The generated answer text (may contain markdown formatting).
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// References to the data sources used to generate the answer.
    /// </summary>
    public List<ChatSourceReference> Sources { get; set; } = new();

    /// <summary>
    /// The detected intent category.
    /// </summary>
    public string Intent { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score of the match (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Suggested follow-up questions the user might ask.
    /// </summary>
    public List<string> SuggestedQuestions { get; set; } = new();
}

/// <summary>
/// A reference to a specific database entity used as a source for the answer.
/// </summary>
public class ChatSourceReference
{
    /// <summary>
    /// The type of entity (e.g., "DraftDocument", "Rule", "Opinion").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// The primary key of the referenced entity.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// A human-readable title for the source.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// A short snippet of the matched content.
    /// </summary>
    public string Snippet { get; set; } = string.Empty;
}

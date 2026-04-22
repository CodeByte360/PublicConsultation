using PublicConsultation.Core.DTOs;
using PublicConsultation.Core.Entities;

namespace PublicConsultation.Core.Interfaces;

/// <summary>
/// Service interface for the DPCS RAG Chatbot.
/// Provides question-answering capabilities using the application's own database.
/// </summary>
public interface IChatbotService
{
    /// <summary>
    /// Process a user's question and return a data-grounded answer.
    /// </summary>
    Task<ChatbotAnswerDto> AskAsync(ChatbotQuestionDto question);

    /// <summary>
    /// Get a list of suggested starter questions based on current data.
    /// </summary>
    Task<List<string>> GetSuggestedQuestionsAsync();

    /// <summary>
    /// Retrieve chat history for a given session.
    /// </summary>
    Task<List<ChatbotConversation>> GetChatHistoryAsync(string sessionId);
}

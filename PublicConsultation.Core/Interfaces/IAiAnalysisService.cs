using PublicConsultation.Core.Entities;

namespace PublicConsultation.Core.Interfaces;

public interface IAiAnalysisService
{
    Task<string> SummarizeOpinionsAsync(List<Opinion> opinions);
    Task<string> AnalyzeSentimentAsync(string text);
    Task<AnalysisResultDto> AnalyzeBatchAsync(List<Opinion> opinions);
    Task<string> AnswerQuestionAsync(Guid documentId, string question);
}

public class AnalysisResultDto
{
    public string Summary { get; set; } = string.Empty;
    public string Sentiment { get; set; } = "Neutral"; // Positive, Negative, Mixed, Neutral
    public List<string> KeyThemes { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}

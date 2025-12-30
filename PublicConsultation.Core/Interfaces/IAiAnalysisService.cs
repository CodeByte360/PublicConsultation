using PublicConsultation.Core.Entities;

namespace PublicConsultation.Core.Interfaces;

public interface IAiAnalysisService
{
    Task<string> SummarizeOpinionsAsync(List<Opinion> opinions);
    Task<string> AnalyzeSentimentAsync(string text);
}

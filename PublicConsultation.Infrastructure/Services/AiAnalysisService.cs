using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;

namespace PublicConsultation.Infrastructure.Services;

public class AiAnalysisService : IAiAnalysisService
{
    public Task<string> AnalyzeSentimentAsync(string text)
    {
        // Stub: Random sentiment for MVP
        var sentiments = new[] { "Positive", "Neutral", "Negative" };
        var random = new Random();
        return Task.FromResult(sentiments[random.Next(sentiments.Length)]);
    }

    public Task<string> SummarizeOpinionsAsync(List<Opinion> opinions)
    {
        if (opinions == null || !opinions.Any())
        {
            return Task.FromResult("No public opinions submitted for this section.");
        }

        // Stub: Mocking an LLM response
        var count = opinions.Count;
        var summary = $"Received {count} citizen opinions. " +
                      "The majority express concern regarding the clarity of the definition. " +
                      "Key suggestions include explicitly defining the scope of 'Digital Security' " +
                      "and reducing the proposed penalties for minor offenses.";

        return Task.FromResult(summary);
    }
}

using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;

namespace PublicConsultation.Infrastructure.Services;

public class AiAnalysisService : IAiAnalysisService
{
    public async Task<AnalysisResultDto> AnalyzeBatchAsync(List<Opinion> opinions)
    {
        if (opinions == null || !opinions.Any())
        {
            return new AnalysisResultDto
            {
                Summary = "No feedback received.",
                Sentiment = "Neutral",
                Recommendation = "Maintain proposed provision."
            };
        }

        var text = string.Join(" ", opinions.Select(o => o.OpinionText.ToLower()));
        var suggestions = string.Join(" ", opinions.Select(o => o.Suggestion?.ToLower() ?? ""));

        // 1. Sentiment Analysis
        int positiveScore = CountKeywords(text, new[] { "good", "support", "excellent", "agree", "benefit", "better", "welcome" });
        int negativeScore = CountKeywords(text, new[] { "bad", "wrong", "oppose", "disagree", "harm", "worse", "reject", "penalty", "heavy" });
        int concernScore = CountKeywords(text, new[] { "concern", "clarify", "unclear", "missing", "vague", "ambiguous", "define" });

        string sentiment = "Neutral";
        if (positiveScore > negativeScore && positiveScore > concernScore) sentiment = "Positive";
        else if (negativeScore > positiveScore && negativeScore > concernScore) sentiment = "Negative";
        else if (concernScore > positiveScore || (negativeScore > 0 && positiveScore > 0)) sentiment = "Mixed";

        // 2. Theme Detection
        var themes = new List<string>();
        if (CountKeywords(text + suggestions, new[] { "privacy", "data", "confidential" }) > 0) themes.Add("Privacy Concerns");
        if (CountKeywords(text + suggestions, new[] { "penalty", "fine", "prison", "jail", "sentence" }) > 0) themes.Add("Punishment Severity");
        if (CountKeywords(text + suggestions, new[] { "freedom", "speech", "expression", "press" }) > 0) themes.Add("Freedom of Expression");
        if (CountKeywords(text + suggestions, new[] { "definition", "meaning", "define", "scope" }) > 0) themes.Add("Definitional Clarity");

        // 3. Recommendation
        string recommendation = "Maintain proposed provision.";
        if (sentiment == "Negative") recommendation = "Significant revision required. Consider softening penalties and increasing oversight.";
        else if (sentiment == "Mixed") recommendation = "Clarification needed. Re-define ambiguous terms based on public feedback.";
        else if (sentiment == "Positive" && themes.Any()) recommendation = "Proceed with proposal, but consider minor tweaks to " + themes.First().ToLower() + ".";

        // 4. Summary
        string summary = await SummarizeOpinionsAsync(opinions);

        return new AnalysisResultDto
        {
            Summary = summary,
            Sentiment = sentiment,
            KeyThemes = themes,
            Recommendation = recommendation
        };
    }

    private int CountKeywords(string text, string[] keywords)
    {
        int count = 0;
        foreach (var word in keywords)
        {
            if (text.Contains(word)) count++;
        }
        return count;
    }

    public Task<string> AnalyzeSentimentAsync(string text)
    {
        var lowerText = text.ToLower();
        int pos = CountKeywords(lowerText, new[] { "good", "support", "agree", "fine" });
        int neg = CountKeywords(lowerText, new[] { "bad", "oppose", "heavy", "wrong" });

        if (pos > neg) return Task.FromResult("Positive");
        if (neg > pos) return Task.FromResult("Negative");
        return Task.FromResult("Neutral");
    }

    public Task<string> SummarizeOpinionsAsync(List<Opinion> opinions)
    {
        if (opinions == null || !opinions.Any())
        {
            return Task.FromResult("No public opinions submitted for this section.");
        }

        var count = opinions.Count;
        var themes = new List<string>();
        var text = string.Join(" ", opinions.Select(o => o.OpinionText.ToLower()));

        if (text.Contains("clear") || text.Contains("unclear")) themes.Add("clarity of definition");
        if (text.Contains("penalty") || text.Contains("punish")) themes.Add("severity of penalties");
        if (text.Contains("digital") || text.Contains("online")) themes.Add("the scope of digital services");

        string summary = $"Based on {count} submission(s), ";
        if (themes.Any())
        {
            summary += $"citizens primarily commented on {string.Join(" and ", themes)}. ";
        }
        else
        {
            summary += "general feedback was provided regarding the general direction of the provision. ";
        }

        summary += "Overall, the feedback " + (CountKeywords(text, new[] { "support", "agree" }) > CountKeywords(text, new[] { "oppose", "heavy" }) ? "leans towards support." : "contains significant reservations.");

        return Task.FromResult(summary);
    }
}

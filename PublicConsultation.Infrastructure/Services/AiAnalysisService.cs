#nullable enable
using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using System.Net.Http.Json;

namespace PublicConsultation.Infrastructure.Services;

public class AiAnalysisService : IAiAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;

    public AiAnalysisService(HttpClient httpClient, IServiceProvider serviceProvider)
    {
        _httpClient = httpClient;
        _serviceProvider = serviceProvider;
    }

    public async Task<AnalysisResultDto> AnalyzeBatchAsync(List<Opinion> opinions)
    {
        if (opinions == null || !opinions.Any())
        {
            return new AnalysisResultDto
            {
                Summary = "No feedback received.",
                Sentiment = "Neutral",
                Recommendation = "Maintain proposed provision.",
                ConsensusScore = 0.5
            };
        }

        // Call Python API for sentiment
        var texts = opinions.Select(o => o.OpinionText ?? string.Empty).ToList();
        List<string> sentiments = new List<string>();
        List<string> apiThemes = new List<string>();
        bool sensitivityAlert = false;

        try
        {
            var response = await _httpClient.PostAsJsonAsync("analyze_batch", new { texts = texts });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<BatchResultDto>();
                if (result != null)
                {
                    sentiments = result.Results.Select(r => r.Sentiment).ToList();
                    apiThemes = result.ExtractedThemes ?? new List<string>();
                    sensitivityAlert = result.SensitivityAlert;
                }
            }
        }
        catch (Exception)
        {
            // Fallback if AI service is down
            sentiments = texts.Select(t => "Neutral").ToList();
        }

        if (!sentiments.Any()) sentiments = texts.Select(t => "Neutral").ToList();

        int positive = sentiments.Count(s => s == "Positive");
        int negative = sentiments.Count(s => s == "Negative");
        int total = opinions.Count;

        string finalSentiment = "Neutral";
        if (positive > negative * 1.5) finalSentiment = "Positive";
        else if (negative > positive * 1.5) finalSentiment = "Negative";
        else if (positive > 0 && negative > 0) finalSentiment = "Mixed";

        double consensusScore = total > 0 ? (double)positive / total : 0;

        // Combine local rule-based themes with AI-extracted themes
        var allText = string.Join(" ", opinions.Select(o => (o.OpinionText ?? string.Empty).ToLower()));
        var localThemes = DetectThemes(allText);
        var combinedThemes = localThemes.Union(apiThemes).Distinct().Take(5).ToList();

        return new AnalysisResultDto
        {
            Summary = await SummarizeOpinionsAsync(opinions),
            Sentiment = finalSentiment,
            KeyThemes = combinedThemes,
            Recommendation = GenerateRecommendation(finalSentiment, combinedThemes, sensitivityAlert),
            ConsensusScore = consensusScore
        };
    }

    public async Task<string> AnalyzeSentimentAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Neutral";

        try
        {
            var response = await _httpClient.PostAsJsonAsync("analyze_sentiment", new { text = text });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SentimentResultDto>();
                if (result != null)
                {
                    // Return "Positive (0.95)" format
                    return $"{result.Sentiment} ({result.Probability:P0})";
                }
            }
        }
        catch
        {
            // Ignore for now, fallback to Neutral
        }
        return "Neutral";
    }

    public async Task<string> SummarizeOpinionAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        try
        {
            var response = await _httpClient.PostAsJsonAsync("summarize", new { text = text });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SummarizeResultDto>();
                if (result != null)
                {
                    return result.Summary;
                }
            }
        }
        catch
        {
            // Fallback to substring
            return text.Length > 200 ? text.Substring(0, 197) + "..." : text;
        }
        return text;
    }

    private List<string> DetectThemes(string text)
    {
        var themes = new List<string>();
        // Simple keyword matching (Client-side lightweight)
        var themesDict = new Dictionary<string, string[]>
        {
            { "Privacy & Data Protection", new[] { "privacy", "data", "encryption", "leak", "surveillance", "tracking", "personal info" } },
            { "Punishment Severity", new[] { "penalty", "prison", "fine", "jail", "sentence", "punish", "harsh", "heavy" } },
            { "Freedom of Expression", new[] { "speech", "press", "freedom", "censorship", "voice", "journalist", "opinion", "expression" } },
            { "Economic Impact", new[] { "cost", "money", "expensive", "tax", "fee", "price", "economy", "business", "market" } },
            { "Definitional Clarity", new[] { "clear", "define", "vague", "ambiguous", "meaning", "uncertain", "unclear", "wording" } },
            { "Human Rights", new[] { "rights", "justice", "fair", "equity", "discrimination", "inclusion", "liberty", "fundamental" } },
            { "Cybersecurity", new[] { "hack", "security", "breach", "threat", "firewall", "attack", "protection", "digital safety" } },
            { "Government Authority", new[] { "power", "control", "ministry", "agency", "official", "regulator", "police", "authority" } }
        };

        foreach (var entry in themesDict)
        {
            if (entry.Value.Any(keyword => text.Contains(keyword))) themes.Add(entry.Key);
        }

        return themes.Take(5).ToList();
    }

    private string GenerateRecommendation(string sentiment, List<string> themes, bool sensitivityAlert)
    {
        if (sensitivityAlert) return "High Risk Alert: Strong negative sentiment detected. Immediate review of " + (themes.Any() ? themes.First() : "key issues") + " is required.";
        if (sentiment == "Negative") return "Critical Revision Required: Re-evaluate penalties and consult on " + (themes.Any() ? themes.First() : "major provisions") + ".";
        if (sentiment == "Mixed") return "Refinement Suggested: Clarify definitions and address concerns regarding " + string.Join(" and ", themes.Take(2)) + ".";
        if (sentiment == "Positive" && themes.Any()) return "Proceed with minor adjustments to ensure " + themes.First() + " is fully protected.";
        return "Proceed with current draft: High public consensus observed.";
    }

    public async Task<string> SummarizeOpinionsAsync(List<Opinion> opinions)
    {
        if (opinions == null || !opinions.Any()) return "No feedback provided.";

        // Aggregate text
        var allText = string.Join(" ", opinions.Select(o => $"{o.OpinionText} {o.Suggestion ?? ""}"));

        if (string.IsNullOrWhiteSpace(allText)) return "No content to summarize.";

        try
        {
            // Try Python AI Summarization first
            var response = await _httpClient.PostAsJsonAsync("summarize", new { text = allText });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SummarizeResultDto>();
                if (result != null && !string.IsNullOrWhiteSpace(result.Summary))
                {
                    return result.Summary;
                }
            }
        }
        catch
        {
            // Fallthrough to manual if service fails
        }

        // Fallback: Simple frequency-based extractive summary
        var sentences = allText.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => s.Length > 10)
                                .ToList();

        if (!sentences.Any()) return "Feedback is too brief for significant analysis.";

        return string.Join(". ", sentences.Take(2)) + ".";
    }

    public async Task<string> AnswerQuestionAsync(Guid documentId, string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return "Please ask a question.";

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var rules = await dbContext.Rules.Where(r => r.DraftDocumentId == documentId).ToListAsync();
        var questionKeywords = Regex.Split(question.ToLower(), @"\W+").Where(w => w.Length > 3).ToList();

        var bestMatch = rules
            .Select(r => new { Rule = r, Score = questionKeywords.Sum(k => (r.ProposedProvision.ToLower().Contains(k) ? 2 : 0) + (r.SectionTitle.ToLower().Contains(k) ? 3 : 0)) })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (bestMatch == null || bestMatch.Score == 0) return "I couldn't find a specific section related to your question. Try rephrasing.";

        return $"Based on **Section {bestMatch.Rule.RuleNumber}: {bestMatch.Rule.SectionTitle}**, the provision states: \"{bestMatch.Rule.ProposedProvision}\".";
    }

    // DTOs for Python API
    private class SentimentResultDto
    {
        public string Sentiment { get; set; } = string.Empty;
        public double Probability { get; set; }
    }
    private class BatchResultDto
    {
        public List<BatchItemDto> Results { get; set; } = new();

        [System.Text.Json.Serialization.JsonPropertyName("extracted_themes")]
        public List<string> ExtractedThemes { get; set; } = new();

        [System.Text.Json.Serialization.JsonPropertyName("sensitivity_alert")]
        public bool SensitivityAlert { get; set; }
    }
    private class BatchItemDto { public string Text { get; set; } = string.Empty; public string Sentiment { get; set; } = string.Empty; }
    private class SummarizeResultDto { public string Summary { get; set; } = string.Empty; }
}

#nullable enable
using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace PublicConsultation.Infrastructure.Services;

public class AiAnalysisService : IAiAnalysisService
{
    private static MLContext _mlContext = new MLContext();
    private static ITransformer? _model;
    private readonly PredictionEngine<SentimentData, SentimentPrediction> _predictionEngine;
    private readonly IServiceProvider _serviceProvider;

    public AiAnalysisService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeModel();

        // ML.NET PredictionEngine is not thread-safe. 
        // We create a new one per Scoped service instance (one per request/session in Blazor Server).
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model!);
    }

    private void InitializeModel()
    {
        if (_model != null) return;

        lock (_mlContext)
        {
            if (_model != null) return;

            var trainingData = GetTrainingData();
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            _model = pipeline.Fit(dataView);
        }
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

        var results = opinions.Select(o => PredictSentiment(o.OpinionText ?? string.Empty)).ToList();

        int positive = results.Count(r => r == "Positive");
        int negative = results.Count(r => r == "Negative");
        int total = opinions.Count;

        string finalSentiment = "Neutral";
        if (positive > negative * 1.5) finalSentiment = "Positive";
        else if (negative > positive * 1.5) finalSentiment = "Negative";
        else if (positive > 0 && negative > 0) finalSentiment = "Mixed";

        double consensusScore = (double)positive / total;

        // Use null-safe text aggregation
        var allText = string.Join(" ", opinions.Select(o => (o.OpinionText ?? string.Empty).ToLower()));
        var themes = DetectThemes(allText);

        return new AnalysisResultDto
        {
            Summary = await SummarizeOpinionsAsync(opinions),
            Sentiment = finalSentiment,
            KeyThemes = themes,
            Recommendation = GenerateRecommendation(finalSentiment, themes),
            ConsensusScore = consensusScore
        };
    }

    private string PredictSentiment(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Neutral";

        lock (_predictionEngine) // Extra safety
        {
            var prediction = _predictionEngine.Predict(new SentimentData { Text = text });
            return prediction.Prediction ? "Positive" : "Negative";
        }
    }

    private List<string> DetectThemes(string text)
    {
        var themes = new List<string>();
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

    private string GenerateRecommendation(string sentiment, List<string> themes)
    {
        if (sentiment == "Negative") return "Critical Revision Required: Re-evaluate penalties and consult on " + (themes.Any() ? themes.First() : "major provisions") + ".";
        if (sentiment == "Mixed") return "Refinement Suggested: Clarify definitions and address concerns regarding " + string.Join(" and ", themes.Take(2)) + ".";
        if (sentiment == "Positive" && themes.Any()) return "Proceed with minor adjustments to ensure " + themes.First() + " is fully protected.";
        return "Proceed with current draft: High public consensus observed.";
    }

    public Task<string> AnalyzeSentimentAsync(string text) => Task.FromResult(PredictSentiment(text));

    public Task<string> SummarizeOpinionsAsync(List<Opinion> opinions)
    {
        if (opinions == null || !opinions.Any()) return Task.FromResult("No feedback provided.");

        var allText = string.Join(" ", opinions.Select(o => $"{o.OpinionText} {o.Suggestion ?? ""}"));
        var sentences = allText.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim())
                                .Where(s => s.Length > 10)
                                .ToList();

        if (!sentences.Any()) return Task.FromResult("Feedback is too brief for significant analysis.");

        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "the", "a", "an", "this", "that", "is", "are", "was", "were", "and", "or", "but", "it", "with", "for", "to", "in", "on", "of", "at", "by", "as" };
        var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var sentence in sentences)
        {
            var words = Regex.Split(sentence.ToLower(), @"\W+").Where(w => w.Length > 3 && !stopWords.Contains(w));
            foreach (var word in words) wordCounts[word] = wordCounts.GetValueOrDefault(word) + 1;
        }

        var rankedSentences = sentences
            .Select(s => new { Sentence = s, Score = Regex.Split(s, @"\W+").Where(w => wordCounts.ContainsKey(w)).Sum(w => wordCounts[w]) })
            .OrderByDescending(x => x.Score)
            .Take(2)
            .Select(x => x.Sentence)
            .ToList();

        return Task.FromResult(string.Join(". ", rankedSentences) + ".");
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

    private List<SentimentData> GetTrainingData() => new()
    {
        new() { Text = "wonderful", Label = true }, new() { Text = "nice", Label = true }, new() { Text = "good", Label = true },
        new() { Text = "great", Label = true }, new() { Text = "excellent", Label = true }, new() { Text = "support", Label = true },
        new() { Text = "I love this", Label = true }, new() { Text = "Perfect solution", Label = true }, new() { Text = "Very helpful", Label = true },
        new() { Text = "I support it", Label = true }, new() { Text = "agree", Label = true }, new() { Text = "Perfect", Label = true },
        new() { Text = "bad", Label = false }, new() { Text = "worst", Label = false }, new() { Text = "terrible", Label = false },
        new() { Text = "horrible", Label = false }, new() { Text = "dislike", Label = false }, new() { Text = "hate", Label = false },
        new() { Text = "reject", Label = false }, new() { Text = "wrong", Label = false }, new() { Text = "oppose", Label = false }
    };

    public class SentimentData
    {
        [LoadColumn(0)] public string Text { get; set; } = string.Empty;
        [LoadColumn(1), ColumnName("Label")] public bool Label { get; set; }
    }

    public class SentimentPrediction : SentimentData
    {
        [ColumnName("PredictedLabel")] public bool Prediction { get; set; }
        public float Probability { get; set; }
        public float Score { get; set; }
    }
}

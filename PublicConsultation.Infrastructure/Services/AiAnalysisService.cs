#nullable enable
using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PublicConsultation.Infrastructure.Services;

public class AiAnalysisService : IAiAnalysisService
{
    private static MLContext _mlContext = new MLContext();
    private static ITransformer? _model;
    private static PredictionEngine<SentimentData, SentimentPrediction>? _predictionEngine;
    private readonly IServiceProvider _serviceProvider;

    public AiAnalysisService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeModel();
    }

    private void InitializeModel()
    {
        if (_model != null) return;

        lock (_mlContext)
        {
            if (_model != null) return;

            // 1. Prepare Training Data
            var trainingData = new List<SentimentData>
            {
                // Positive
                new() { Text = "wonderful", Label = true },
                new() { Text = "nice", Label = true },
                new() { Text = "good", Label = true },
                new() { Text = "great", Label = true },
                new() { Text = "excellent", Label = true },
                new() { Text = "support", Label = true },
                new() { Text = "I love this", Label = true },
                new() { Text = "Perfect solution", Label = true },
                new() { Text = "Very helpful", Label = true },
                new() { Text = "This is a great proposal, I support it.", Label = true },
                new() { Text = "Excellent work on the digital safety aspects.", Label = true },
                new() { Text = "I really like this new rule.", Label = true },
                new() { Text = "This will benefit the public greatly.", Label = true },
                new() { Text = "I support the ministry's decision.", Label = true },
                new() { Text = "It's okay but could be clearer.", Label = true },
                new() { Text = "This is fine.", Label = true },
                new() { Text = "Good job.", Label = true },
                new() { Text = "Absolutely support.", Label = true },

                // Negative
                new() { Text = "bad", Label = false },
                new() { Text = "worst", Label = false },
                new() { Text = "terrible", Label = false },
                new() { Text = "horrible", Label = false },
                new() { Text = "dislike", Label = false },
                new() { Text = "hate", Label = false },
                new() { Text = "reject", Label = false },
                new() { Text = "wrong", Label = false },
                new() { Text = "This is bad and will cause harm.", Label = false },
                new() { Text = "I strongly oppose this heavy penalty.", Label = false },
                new() { Text = "The prison sentence is too long.", Label = false },
                new() { Text = "This violates freedom of speech.", Label = false },
                new() { Text = "I reject this proposal completely.", Label = false },
                new() { Text = "I disagree with section 5.", Label = false },
                new() { Text = "Too expensive.", Label = false },
                new() { Text = "Not fair.", Label = false },
                new() { Text = "Poorly written.", Label = false },
                new() { Text = "I hate this rule.", Label = false },
                new() { Text = "This is a disaster.", Label = false }
            };

            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // 2. Define ML Pipeline
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            // 3. Train Model
            _model = pipeline.Fit(dataView);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_model);
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
                Recommendation = "Maintain proposed provision."
            };
        }

        var results = opinions.Select(o => PredictSentiment(o.OpinionText)).ToList();

        // Count results
        int positive = results.Count(r => r == "Positive");
        int negative = results.Count(r => r == "Negative");

        string finalSentiment = "Neutral";
        if (positive > negative * 1.5) finalSentiment = "Positive";
        else if (negative > positive * 1.5) finalSentiment = "Negative";
        else if (positive > 0 && negative > 0) finalSentiment = "Mixed";

        // Heuristic based Themes and Recommendation
        var text = string.Join(" ", opinions.Select(o => o.OpinionText.ToLower()));
        var themes = DetectThemes(text);

        return new AnalysisResultDto
        {
            Summary = await SummarizeOpinionsAsync(opinions),
            Sentiment = finalSentiment,
            KeyThemes = themes,
            Recommendation = GenerateRecommendation(finalSentiment, themes)
        };
    }

    private string PredictSentiment(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "Neutral";
        var prediction = _predictionEngine!.Predict(new SentimentData { Text = text });
        return prediction.Prediction ? "Positive" : "Negative";
    }

    private List<string> DetectThemes(string text)
    {
        var themes = new List<string>();
        if (text.Contains("privacy") || text.Contains("data")) themes.Add("Privacy Concerns");
        if (text.Contains("penalty") || text.Contains("prison") || text.Contains("fine")) themes.Add("Punishment Severity");
        if (text.Contains("speech") || text.Contains("press") || text.Contains("freedom")) themes.Add("Freedom of Expression");
        if (text.Contains("clear") || text.Contains("define") || text.Contains("vague")) themes.Add("Definitional Clarity");
        return themes;
    }

    private string GenerateRecommendation(string sentiment, List<string> themes)
    {
        if (sentiment == "Negative") return "Significant revision required. Re-evaluate proposed penalties.";
        if (sentiment == "Mixed") return "Clarification needed. Address concerns regarding " + string.Join(", ", themes.Take(2));
        if (sentiment == "Positive" && themes.Any()) return "Proceed with proposal, with minor wording adjustments.";
        return "Proceed with current draft.";
    }

    public Task<string> AnalyzeSentimentAsync(string text)
    {
        return Task.FromResult(PredictSentiment(text));
    }

    public Task<string> SummarizeOpinionsAsync(List<Opinion> opinions)
    {
        if (opinions == null || !opinions.Any()) return Task.FromResult("No feedback.");

        var count = opinions.Count;
        var text = string.Join(". ", opinions.Select(o => o.OpinionText));

        // Simple extractive
        var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var mainPoint = sentences.FirstOrDefault()?.Trim() ?? "Feedback received.";

        return Task.FromResult($"Analysis of {count} submission(s): {mainPoint}");
    }

    public async Task<string> AnswerQuestionAsync(Guid documentId, string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return "Please ask a question.";

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var rules = await dbContext.Rules
            .Where(r => r.DraftDocumentId == documentId)
            .ToListAsync();

        // Simple keyword-based retrieval
        var keywords = question.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3).ToList();

        var bestMatch = rules
            .Select(r => new { Rule = r, Score = keywords.Count(k => r.ProposedProvision.ToLower().Contains(k) || r.SectionTitle.ToLower().Contains(k)) })
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (bestMatch == null || bestMatch.Score == 0)
        {
            return "I couldn't find a specific section related to your question in this draft. Could you try rephrasing?";
        }

        return $"Based on **Section {bestMatch.Rule.RuleNumber}: {bestMatch.Rule.SectionTitle}**, the provision states: \"{bestMatch.Rule.ProposedProvision}\". \n\nDoes this help with your question?";
    }

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

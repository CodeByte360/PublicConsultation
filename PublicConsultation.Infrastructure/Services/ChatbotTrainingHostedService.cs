#nullable enable
using PublicConsultation.Core.Entities;
using PublicConsultation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace PublicConsultation.Infrastructure.Services;

/// <summary>
/// Background service that automatically trains the chatbot by:
/// 1. Extracting keywords from all database entities (documents, rules, opinions, ministries)
/// 2. Learning from past conversation patterns (which questions matched which intents)
/// 3. Building a weighted keyword index (TF-IDF style)
/// 4. Runs every 24 hours automatically
/// </summary>
public class ChatbotTrainingHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatbotTrainingHostedService> _logger;
    private static readonly TimeSpan TrainingInterval = TimeSpan.FromHours(24);

    // Stop words to exclude from keyword extraction
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "is", "are", "was", "were", "be", "been", "being",
        "have", "has", "had", "do", "does", "did", "will", "would", "could",
        "should", "may", "might", "shall", "can", "need", "must",
        "and", "or", "but", "not", "no", "nor", "so", "yet",
        "in", "on", "at", "to", "for", "of", "with", "by", "from", "up",
        "about", "into", "through", "during", "before", "after", "above", "below",
        "this", "that", "these", "those", "it", "its",
        "i", "me", "my", "we", "our", "you", "your", "he", "she", "they", "them",
        "what", "which", "who", "whom", "where", "when", "why", "how",
        "all", "each", "every", "both", "few", "more", "most", "some", "any",
        "also", "than", "too", "very", "just", "only", "own", "same", "such"
    };

    public ChatbotTrainingHostedService(IServiceProvider serviceProvider, ILogger<ChatbotTrainingHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Chatbot Training Service started. First training will begin in 30 seconds.");

        // Wait 30 seconds after app start to let DB initialize
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Chatbot auto-training started at {Time}", DateTime.UtcNow);
                await RunTrainingAsync(stoppingToken);
                _logger.LogInformation("Chatbot auto-training completed successfully at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during chatbot auto-training");
            }

            await Task.Delay(TrainingInterval, stoppingToken);
        }
    }

    private async Task RunTrainingAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var newIndex = new List<ChatbotKnowledgeIndex>();

        // ── Phase 1: Extract keywords from DraftDocuments ─────────────
        var docs = await db.DraftDocuments.AsNoTracking().ToListAsync(ct);
        foreach (var doc in docs)
        {
            var titleWords = ExtractWords(doc.Title);
            foreach (var word in titleWords)
                AddToIndex(newIndex, word, "DocumentQuery", "DraftDocument", doc.Oid, 3.0);

            var descWords = ExtractWords(doc.Description ?? "");
            foreach (var word in descWords)
                AddToIndex(newIndex, word, "DocumentQuery", "DraftDocument", doc.Oid, 1.5);

            var deptWords = ExtractWords(doc.MinistryOrDepartment);
            foreach (var word in deptWords)
                AddToIndex(newIndex, word, "MinistryQuery", "DraftDocument", doc.Oid, 2.0);
        }

        // ── Phase 2: Extract keywords from Rules ──────────────────────
        var rules = await db.Rules.AsNoTracking().ToListAsync(ct);
        foreach (var rule in rules)
        {
            var titleWords = ExtractWords(rule.SectionTitle);
            foreach (var word in titleWords)
                AddToIndex(newIndex, word, "RuleQuery", "Rule", rule.Oid, 3.0);

            var provisionWords = ExtractWords(rule.ProposedProvision);
            foreach (var word in provisionWords)
                AddToIndex(newIndex, word, "RuleQuery", "Rule", rule.Oid, 2.0);

            var existingWords = ExtractWords(rule.ExistingProvision);
            foreach (var word in existingWords)
                AddToIndex(newIndex, word, "RuleQuery", "Rule", rule.Oid, 1.0);

            // Index rule numbers directly
            if (!string.IsNullOrEmpty(rule.RuleNumber))
                AddToIndex(newIndex, rule.RuleNumber.ToLower(), "RuleQuery", "Rule", rule.Oid, 5.0);
        }

        // ── Phase 3: Extract keywords from Opinions ───────────────────
        var opinions = await db.Opinions.AsNoTracking().ToListAsync(ct);
        foreach (var opinion in opinions)
        {
            var words = ExtractWords(opinion.OpinionText);
            foreach (var word in words)
                AddToIndex(newIndex, word, "OpinionQuery", "Opinion", opinion.Oid, 1.5);

            if (!string.IsNullOrEmpty(opinion.Suggestion))
            {
                var sugWords = ExtractWords(opinion.Suggestion);
                foreach (var word in sugWords)
                    AddToIndex(newIndex, word, "OpinionQuery", "Opinion", opinion.Oid, 1.0);
            }
        }

        // ── Phase 4: Extract keywords from Ministries ─────────────────
        var ministries = await db.Ministries.AsNoTracking().ToListAsync(ct);
        foreach (var ministry in ministries)
        {
            var nameWords = ExtractWords(ministry.Name);
            foreach (var word in nameWords)
                AddToIndex(newIndex, word, "MinistryQuery", "Ministry", ministry.Oid, 4.0);

            var descWords = ExtractWords(ministry.Description);
            foreach (var word in descWords)
                AddToIndex(newIndex, word, "MinistryQuery", "Ministry", ministry.Oid, 1.5);
        }

        // ── Phase 5: Learn from past conversations ────────────────────
        var conversations = await db.ChatbotConversations.AsNoTracking().ToListAsync(ct);
        var successfulConversations = conversations.Where(c => c.Confidence > 0.5 && c.Intent != "Unknown" && c.Intent != "NoResults").ToList();

        foreach (var conv in successfulConversations)
        {
            var questionWords = ExtractWords(conv.UserMessage);
            foreach (var word in questionWords)
            {
                // Learn: this word was used in a question that matched this intent
                double learnWeight = conv.Confidence * 2.0;
                AddToIndex(newIndex, word, conv.Intent, "Conversation", null, learnWeight);
            }
        }

        // ── Phase 6: Calculate TF-IDF weights ─────────────────────────
        // Group by keyword, merge weights
        var merged = newIndex
            .GroupBy(k => new { k.Keyword, k.Intent })
            .Select(g => new ChatbotKnowledgeIndex
            {
                Oid = Guid.NewGuid(),
                Keyword = g.Key.Keyword,
                Intent = g.Key.Intent,
                SourceEntity = g.First().SourceEntity,
                SourceEntityId = g.First().SourceEntityId,
                Weight = g.Sum(x => x.Weight) / Math.Log2(g.Count() + 2), // TF-IDF-like normalization
                HitCount = 0,
                LastTrainedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            })
            .Where(k => k.Weight > 0.5) // Filter out low-weight noise
            .OrderByDescending(k => k.Weight)
            .Take(5000) // Cap to prevent unbounded growth
            .ToList();

        // Preserve hit counts from existing index
        var existingIndex = await db.ChatbotKnowledgeIndex.ToListAsync(ct);
        foreach (var entry in merged)
        {
            var existing = existingIndex.FirstOrDefault(e => e.Keyword == entry.Keyword && e.Intent == entry.Intent);
            if (existing != null)
                entry.HitCount = existing.HitCount; // Carry forward hit counts
        }

        // ── Phase 7: Replace old index with new one ───────────────────
        db.ChatbotKnowledgeIndex.RemoveRange(existingIndex);
        await db.SaveChangesAsync(ct);

        db.ChatbotKnowledgeIndex.AddRange(merged);
        await db.SaveChangesAsync(ct);

        _logger.LogInformation("Training complete: indexed {Count} keyword entries from {Docs} docs, {Rules} rules, {Opinions} opinions, {Ministries} ministries, {Convos} conversations",
            merged.Count, docs.Count, rules.Count, opinions.Count, ministries.Count, successfulConversations.Count);
    }

    private List<string> ExtractWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new();

        return Regex.Split(text.ToLower(), @"\W+")
            .Where(w => w.Length > 2 && !StopWords.Contains(w))
            .Distinct()
            .ToList();
    }

    private void AddToIndex(List<ChatbotKnowledgeIndex> index, string keyword, string intent, string sourceEntity, Guid? sourceId, double weight)
    {
        index.Add(new ChatbotKnowledgeIndex
        {
            Oid = Guid.NewGuid(),
            Keyword = keyword,
            Intent = intent,
            SourceEntity = sourceEntity,
            SourceEntityId = sourceId,
            Weight = weight,
            LastTrainedDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        });
    }
}

#nullable enable
using PublicConsultation.Core.DTOs;
using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace PublicConsultation.Infrastructure.Services;

public class ChatbotService : IChatbotService
{
    private readonly IServiceProvider _serviceProvider;

    // Intent definitions with keywords and patterns (English + Bengali + Banglish)
    private static readonly Dictionary<string, string[]> IntentKeywords = new()
    {
        ["DocumentQuery"] = new[] { "draft", "document", "law", "legislation", "act", "bill", "policy", "regulation", "published", "upload", "আইন", "খসড়া", "নথি", "বিল", "নীতি", "দলিল", "ain", "khosra", "nothi", "dolil", "niti", "prostab", "aain", "koshora" },
        ["RuleQuery"] = new[] { "section", "rule", "provision", "clause", "article", "amendment", "proposed", "existing", "ধারা", "বিধি", "নিয়ম", "সংশোধন", "প্রস্তাবিত", "dhara", "bidhi", "niyom", "songshodhon", "prostabito", "dharay", "niyome" },
        ["OpinionQuery"] = new[] { "opinion", "feedback", "public", "comment", "citizen", "suggestion", "review", "submit", "মতামত", "মন্তব্য", "পরামর্শ", "নাগরিক", "জনমত", "motamot", "montobbo", "poramorsho", "nagorik", "jonogon", "jonmot", "mota", "jonomot" },
        ["MinistryQuery"] = new[] { "ministry", "department", "organization", "who published", "responsible", "authority", "মন্ত্রণালয়", "বিভাগ", "দপ্তর", "montronaloy", "bibhag", "doptor", "montri", "sarkar", "government" },
        ["ConsultationStatus"] = new[] { "active", "open", "closed", "deadline", "when", "date", "status", "expire", "running", "ongoing", "চলমান", "সক্রিয়", "বন্ধ", "সময়সীমা", "তারিখ", "choloman", "sokriyo", "bondho", "shomoy", "tarikh", "kokhon", "kobe", "sesh" },
        ["StatisticsQuery"] = new[] { "how many", "count", "total", "number of", "statistics", "stat", "summary", "overview", "কতগুলো", "মোট", "সংখ্যা", "পরিসংখ্যান", "kotogulo", "mot", "songkha", "total", "koto", "koyta" },
        ["SystemHelp"] = new[] { "how to", "what is dpcs", "help", "feature", "about", "system", "platform", "guide", "tutorial", "কিভাবে", "সাহায্য", "কি", "বৈশিষ্ট্য", "kivabe", "sahajjo", "ki", "kemon", "boishistho", "dpcs ki", "eta ki", "kibhabe" },
        ["GeneralGreeting"] = new[] { "hi", "hello", "hey", "thanks", "thank you", "bye", "good morning", "good evening", "assalamu", "আসসালামু", "ধন্যবাদ", "হ্যালো", "শুভ", "assalamualaikum", "dhonnobad", "shuvo", "nomoskar", "salamalaikum", "walaikum" }
    };

    public ChatbotService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ChatbotAnswerDto> AskAsync(ChatbotQuestionDto question)
    {
        if (string.IsNullOrWhiteSpace(question.Question))
            return new ChatbotAnswerDto { Answer = "Please type a question to get started.", Intent = "Empty", Confidence = 0 };

        var (intent, confidence) = DetectIntent(question.Question);
        var result = intent switch
        {
            "DocumentQuery" => await HandleDocumentQuery(question.Question),
            "RuleQuery" => await HandleRuleQuery(question.Question),
            "OpinionQuery" => await HandleOpinionQuery(question.Question),
            "MinistryQuery" => await HandleMinistryQuery(question.Question),
            "ConsultationStatus" => await HandleConsultationStatusQuery(question.Question),
            "StatisticsQuery" => await HandleStatisticsQuery(),
            "SystemHelp" => HandleSystemHelp(question.Question),
            "GeneralGreeting" => HandleGreeting(question.Question),
            _ => await HandleUniversalSearch(question.Question) // Search ALL tables for any question
        };

        result.Intent = intent;
        result.Confidence = confidence;

        // Persist conversation
        await SaveConversation(question, result);

        return result;
    }

    public async Task<List<string>> GetSuggestedQuestionsAsync()
    {
        var suggestions = new List<string>
        {
            "What draft documents are currently active?",
            "How many public opinions have been submitted?",
            "What is DPCS?",
            "Show me the latest consultations",
            "Which ministries have published documents?"
        };

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var latestDoc = await db.DraftDocuments.OrderByDescending(d => d.CreatedDate).FirstOrDefaultAsync();
            if (latestDoc != null)
                suggestions.Insert(1, $"Tell me about \"{latestDoc.Title}\"");
        }
        catch { /* keep default suggestions */ }

        return suggestions.Take(5).ToList();
    }

    public async Task<List<ChatbotConversation>> GetChatHistoryAsync(string sessionId)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.ChatbotConversations
            .Where(c => c.SessionId == sessionId)
            .OrderBy(c => c.CreatedDate)
            .ToListAsync();
    }

    // ── Intent Detection (uses static keywords + learned index) ─────
    private (string intent, double confidence) DetectIntent(string question)
    {
        var lower = question.ToLower().Trim();
        string bestIntent = "Unknown";
        double bestScore = 0;

        // Step 1: Check static keyword definitions
        foreach (var (intent, keywords) in IntentKeywords)
        {
            int matchCount = keywords.Count(k => lower.Contains(k));
            if (matchCount > 0)
            {
                double score = (double)matchCount / keywords.Length;
                if (keywords.Any(k => k.Length > 4 && lower.Contains(k))) score += 0.1;
                if (score > bestScore) { bestScore = score; bestIntent = intent; }
            }
        }

        // Step 2: Check auto-trained knowledge index (learned from DB content + past conversations)
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var questionWords = Regex.Split(lower, @"\W+").Where(w => w.Length > 2).ToList();

            if (questionWords.Any())
            {
                var matchedEntries = db.ChatbotKnowledgeIndex
                    .Where(k => questionWords.Contains(k.Keyword))
                    .ToList();

                if (matchedEntries.Any())
                {
                    // Group by intent and sum weights
                    var intentScores = matchedEntries
                        .GroupBy(e => e.Intent)
                        .Select(g => new { Intent = g.Key, Score = g.Sum(e => e.Weight) / 10.0 })
                        .OrderByDescending(x => x.Score)
                        .First();

                    // If learned score beats static score, use it
                    if (intentScores.Score > bestScore)
                    {
                        bestScore = intentScores.Score;
                        bestIntent = intentScores.Intent;
                    }

                    // Update hit counts for matched keywords (async fire-and-forget)
                    foreach (var entry in matchedEntries)
                        entry.HitCount++;
                    db.SaveChanges();
                }
            }
        }
        catch { /* Don't let index lookup failures break intent detection */ }

        return (bestIntent, Math.Min(bestScore + 0.3, 1.0));
    }

    // ── Document Queries ──────────────────────────────────────────────
    private async Task<ChatbotAnswerDto> HandleDocumentQuery(string question)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var keywords = ExtractKeywords(question);
        
        var query = db.DraftDocuments.Include(d => d.Ministry).AsNoTracking().AsQueryable();
        
        var lowerQuestion = question.ToLower();
        var now = DateTime.UtcNow;
        bool wantsActive = lowerQuestion.Contains("open") || lowerQuestion.Contains("active") || lowerQuestion.Contains("running") || lowerQuestion.Contains("চলমান") || lowerQuestion.Contains("সক্রিয়");
        bool wantsClosed = lowerQuestion.Contains("closed") || lowerQuestion.Contains("expired") || lowerQuestion.Contains("বন্ধ");

        if (wantsActive)
        {
            query = query.Where(d => d.Status == "Published" && d.ConsultationEndDate > now);
        }
        else if (wantsClosed)
        {
            query = query.Where(d => d.Status == "Closed" || d.ConsultationEndDate <= now);
        }

        var docs = await query.ToListAsync();

        var scored = docs.Select(d => new
        {
            Doc = d,
            Score = keywords.Sum(k =>
                (d.Title.Contains(k, StringComparison.OrdinalIgnoreCase) ? 3 : 0) +
                ((d.Description ?? "").Contains(k, StringComparison.OrdinalIgnoreCase) ? 2 : 0) +
                (d.MinistryOrDepartment.Contains(k, StringComparison.OrdinalIgnoreCase) ? 1 : 0))
        }).Where(x => x.Score > 0).OrderByDescending(x => x.Score).Take(3).ToList();

        if (!scored.Any())
        {
            // Return all documents summary
            if (!docs.Any()) return new ChatbotAnswerDto { Answer = "There are no draft documents in the system yet." };
            var summary = string.Join("\n", docs.Take(5).Select((d, i) => $"**{i + 1}. {d.Title}** — Status: {d.Status}, Ministry: {d.MinistryOrDepartment}"));
            return new ChatbotAnswerDto
            {
                Answer = $"Here are the latest draft documents:\n\n{summary}\n\n📄 Total: **{docs.Count}** documents in the system.",
                Sources = docs.Take(5).Select(d => new ChatSourceReference { EntityType = "DraftDocument", EntityId = d.Oid, Title = d.Title, Snippet = d.Status }).ToList(),
                SuggestedQuestions = new() { "Which consultations are currently active?", "How many opinions were submitted?" }
            };
        }

        var top = scored.First().Doc;
        var lines = scored.Select((s, i) => $"**{i + 1}. {s.Doc.Title}**\n   - Ministry: {s.Doc.MinistryOrDepartment}\n   - Status: {s.Doc.Status}\n   - Consultation: {s.Doc.ConsultationStartDate:dd MMM yyyy} → {s.Doc.ConsultationEndDate:dd MMM yyyy}\n   - {(string.IsNullOrEmpty(s.Doc.Description) ? "" : $"Description: {Truncate(s.Doc.Description, 120)}")}");

        return new ChatbotAnswerDto
        {
            Answer = $"I found **{scored.Count}** matching document(s):\n\n{string.Join("\n\n", lines)}",
            Sources = scored.Select(s => new ChatSourceReference { EntityType = "DraftDocument", EntityId = s.Doc.Oid, Title = s.Doc.Title, Snippet = s.Doc.Status }).ToList(),
            SuggestedQuestions = new() { $"What are the rules in \"{top.Title}\"?", "Show active consultations" }
        };
    }

    // ── Rule Queries ──────────────────────────────────────────────────
    private async Task<ChatbotAnswerDto> HandleRuleQuery(string question)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var keywords = ExtractKeywords(question);
        var rules = await db.Rules.Include(r => r.DraftDocument).AsNoTracking().ToListAsync();

        var scored = rules.Select(r => new
        {
            Rule = r,
            Score = keywords.Sum(k =>
                (r.SectionTitle.Contains(k, StringComparison.OrdinalIgnoreCase) ? 3 : 0) +
                (r.ProposedProvision.Contains(k, StringComparison.OrdinalIgnoreCase) ? 2 : 0) +
                (r.RuleNumber.Contains(k, StringComparison.OrdinalIgnoreCase) ? 4 : 0) +
                (r.ExistingProvision.Contains(k, StringComparison.OrdinalIgnoreCase) ? 1 : 0))
        }).Where(x => x.Score > 0).OrderByDescending(x => x.Score).Take(3).ToList();

        if (!scored.Any())
        {
            var totalRules = rules.Count;
            return new ChatbotAnswerDto
            {
                Answer = $"I couldn't find a specific rule matching your query. There are **{totalRules}** rules across all documents. Try asking about a specific section number or title.",
                SuggestedQuestions = rules.Take(3).Select(r => $"What does Section {r.RuleNumber} say?").ToList()
            };
        }

        var lines = scored.Select((s, i) =>
            $"**Section {s.Rule.RuleNumber}: {s.Rule.SectionTitle}**\n" +
            $"   📄 Document: {s.Rule.DraftDocument?.Title ?? "N/A"}\n" +
            $"   📝 Proposed: \"{Truncate(s.Rule.ProposedProvision, 200)}\"\n" +
            $"   {(string.IsNullOrEmpty(s.Rule.ExistingProvision) ? "" : $"📋 Existing: \"{Truncate(s.Rule.ExistingProvision, 150)}\"")}");

        return new ChatbotAnswerDto
        {
            Answer = $"Based on my search, here are the most relevant rules:\n\n{string.Join("\n\n", lines)}",
            Sources = scored.Select(s => new ChatSourceReference { EntityType = "Rule", EntityId = s.Rule.Oid, Title = $"Section {s.Rule.RuleNumber}", Snippet = Truncate(s.Rule.SectionTitle, 80) }).ToList(),
            SuggestedQuestions = new() { "What are the public opinions on this rule?", "Show all active consultations" }
        };
    }

    // ── Opinion Queries ───────────────────────────────────────────────
    private async Task<ChatbotAnswerDto> HandleOpinionQuery(string question)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var totalOpinions = await db.Opinions.CountAsync();
        var recentOpinions = await db.Opinions.Include(o => o.Rule).ThenInclude(r => r!.DraftDocument)
            .OrderByDescending(o => o.CreatedDate).Take(5).AsNoTracking().ToListAsync();

        var sentimentCounts = await db.Opinions
            .GroupBy(o => o.Sentiment ?? "Unknown")
            .Select(g => new { Sentiment = g.Key, Count = g.Count() })
            .ToListAsync();

        var sentimentSummary = sentimentCounts.Any()
            ? string.Join(", ", sentimentCounts.Select(s => $"{s.Sentiment}: **{s.Count}**"))
            : "No sentiment data available";

        var recentLines = recentOpinions.Select((o, i) =>
            $"{i + 1}. \"{Truncate(o.OpinionText, 100)}\" — *{o.Sentiment ?? "Pending"}* (on {o.Rule?.SectionTitle ?? "Unknown Rule"})");

        return new ChatbotAnswerDto
        {
            Answer = $"📊 **Public Opinion Summary:**\n\n" +
                     $"- Total opinions submitted: **{totalOpinions}**\n" +
                     $"- Sentiment breakdown: {sentimentSummary}\n\n" +
                     $"**Recent Feedback:**\n{string.Join("\n", recentLines)}",
            Sources = recentOpinions.Select(o => new ChatSourceReference { EntityType = "Opinion", EntityId = o.Oid, Title = "Public Opinion", Snippet = Truncate(o.OpinionText, 60) }).ToList(),
            SuggestedQuestions = new() { "Which rules received the most feedback?", "Show active consultations" }
        };
    }

    // ── Ministry Queries ──────────────────────────────────────────────
    private async Task<ChatbotAnswerDto> HandleMinistryQuery(string question)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var ministries = await db.Ministries.AsNoTracking().ToListAsync();
        var docs = await db.DraftDocuments.AsNoTracking().ToListAsync();

        if (!ministries.Any())
            return new ChatbotAnswerDto { Answer = "No ministries have been registered in the system yet." };

        var lines = ministries.Select(m =>
        {
            var docCount = docs.Count(d => d.MinistryId == m.Oid);
            return $"🏛️ **{m.Name}** — {(string.IsNullOrEmpty(m.Description) ? "No description" : m.Description)} ({docCount} document{(docCount != 1 ? "s" : "")})";
        });

        return new ChatbotAnswerDto
        {
            Answer = $"Here are the registered ministries:\n\n{string.Join("\n", lines)}\n\n📋 Total: **{ministries.Count}** ministries",
            Sources = ministries.Select(m => new ChatSourceReference { EntityType = "Ministry", EntityId = m.Oid, Title = m.Name, Snippet = m.Description }).ToList(),
            SuggestedQuestions = new() { "What documents are currently active?", "Show consultation statistics" }
        };
    }

    // ── Consultation Status ───────────────────────────────────────────
    private async Task<ChatbotAnswerDto> HandleConsultationStatusQuery(string question)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow;
        var docs = await db.DraftDocuments.Include(d => d.Ministry).AsNoTracking().ToListAsync();

        var active = docs.Where(d => d.Status == "Published" && d.ConsultationEndDate > now).ToList();
        var closed = docs.Where(d => d.Status == "Closed" || d.ConsultationEndDate <= now).ToList();
        var draft = docs.Where(d => d.Status == "Draft").ToList();

        var activeLines = active.Any()
            ? string.Join("\n", active.Select((d, i) => $"  {i + 1}. **{d.Title}** — Deadline: {d.ConsultationEndDate:dd MMM yyyy} ({(d.ConsultationEndDate - now).Days} days left)"))
            : "  None currently active.";

        return new ChatbotAnswerDto
        {
            Answer = $"📅 **Consultation Status Overview:**\n\n" +
                     $"🟢 **Active ({active.Count}):**\n{activeLines}\n\n" +
                     $"🔴 **Closed:** {closed.Count} consultation(s)\n" +
                     $"📝 **Draft:** {draft.Count} document(s) pending publication",
            Sources = active.Select(d => new ChatSourceReference { EntityType = "DraftDocument", EntityId = d.Oid, Title = d.Title, Snippet = $"Ends {d.ConsultationEndDate:dd MMM yyyy}" }).ToList(),
            SuggestedQuestions = new() { "How many opinions have been submitted?", "Which ministries have published documents?" }
        };
    }

    // ── Statistics ─────────────────────────────────────────────────────
    private async Task<ChatbotAnswerDto> HandleStatisticsQuery()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var docCount = await db.DraftDocuments.CountAsync();
        var ruleCount = await db.Rules.CountAsync();
        var opinionCount = await db.Opinions.CountAsync();
        var userCount = await db.UserAccounts.CountAsync();
        var ministryCount = await db.Ministries.CountAsync();

        return new ChatbotAnswerDto
        {
            Answer = $"📊 **DPCS System Statistics:**\n\n" +
                     $"| Metric | Count |\n|--------|-------|\n" +
                     $"| 📄 Draft Documents | **{docCount}** |\n" +
                     $"| 📋 Rules/Sections | **{ruleCount}** |\n" +
                     $"| 💬 Public Opinions | **{opinionCount}** |\n" +
                     $"| 👥 Registered Users | **{userCount}** |\n" +
                     $"| 🏛️ Ministries | **{ministryCount}** |",
            SuggestedQuestions = new() { "Show active consultations", "What are the latest opinions?", "Which ministry has the most documents?" }
        };
    }

    // ── System Help ───────────────────────────────────────────────────
    private ChatbotAnswerDto HandleSystemHelp(string question)
    {
        return new ChatbotAnswerDto
        {
            Answer = "🏛️ **Welcome to the Digital Public Consultation System (DPCS)!**\n\n" +
                     "DPCS is a government platform that enables citizens to participate in the legislative process. Here's what you can do:\n\n" +
                     "1. **Browse Draft Documents** — View proposed laws and regulations before they are enacted\n" +
                     "2. **Submit Opinions** — Provide feedback, suggestions, and objections on specific rules\n" +
                     "3. **Track Consultations** — See which consultations are active, their deadlines, and status\n" +
                     "4. **View AI Analysis** — See sentiment analysis and theme extraction from public feedback\n" +
                     "5. **Transparency Log** — Access audit trails showing how citizen input influenced legislation\n\n" +
                     "**Ask me anything** about draft documents, rules, public opinions, ministries, or consultation statuses!",
            SuggestedQuestions = new() { "What documents are currently active?", "How many opinions have been submitted?", "Show system statistics" },
            Confidence = 1.0
        };
    }

    // ── Greetings ─────────────────────────────────────────────────────
    private ChatbotAnswerDto HandleGreeting(string question)
    {
        var lower = question.ToLower();
        string response;

        if (lower.Contains("bye") || lower.Contains("goodbye"))
            response = "Goodbye! 👋 Feel free to come back anytime you have questions about DPCS.";
        else if (lower.Contains("thank"))
            response = "You're welcome! 😊 Is there anything else I can help you with?";
        else
            response = "Hello! 👋 I'm the **DPCS AI Assistant**. I can help you with information about draft documents, rules, public opinions, consultation statuses, and more.\n\nWhat would you like to know?";

        return new ChatbotAnswerDto
        {
            Answer = response,
            SuggestedQuestions = new() { "What is DPCS?", "Show active consultations", "How many opinions have been submitted?" },
            Confidence = 1.0
        };
    }

    // ── Universal Search (Fallback for ANY question) ──────────────────
    private async Task<ChatbotAnswerDto> HandleUniversalSearch(string question)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var keywords = ExtractKeywords(question);
        if (!keywords.Any())
        {
            return new ChatbotAnswerDto
            {
                Answer = "I'd love to help! You can ask me questions like:\n\n" +
                         "• \"What draft documents are active?\"\n" +
                         "• \"Tell me about Section 57\"\n" +
                         "• \"How many opinions were submitted?\"\n" +
                         "• \"Which ministry published the latest document?\"\n" +
                         "• Or just type any topic and I'll search the entire database!",
                Intent = "Help", Confidence = 0.5,
                SuggestedQuestions = new() { "Show system statistics", "What is DPCS?", "Show active consultations" }
            };
        }

        var results = new List<(string type, string title, string snippet, Guid id, int score)>();

        // Search DraftDocuments
        var docs = await db.DraftDocuments.AsNoTracking().ToListAsync();
        var lowerQuestion = question.ToLower();
        var now = DateTime.UtcNow;
        bool wantsActive = lowerQuestion.Contains("open") || lowerQuestion.Contains("active") || lowerQuestion.Contains("running") || lowerQuestion.Contains("চলমান") || lowerQuestion.Contains("সক্রিয়");
        bool wantsClosed = lowerQuestion.Contains("closed") || lowerQuestion.Contains("expired") || lowerQuestion.Contains("বন্ধ");

        foreach (var d in docs)
        {
            if (wantsActive && (d.Status != "Published" || d.ConsultationEndDate <= now)) continue;
            if (wantsClosed && (d.Status != "Closed" && d.ConsultationEndDate > now)) continue;

            int score = keywords.Sum(k =>
                (d.Title.Contains(k, StringComparison.OrdinalIgnoreCase) ? 3 : 0) +
                ((d.Description ?? "").Contains(k, StringComparison.OrdinalIgnoreCase) ? 2 : 0) +
                (d.MinistryOrDepartment.Contains(k, StringComparison.OrdinalIgnoreCase) ? 1 : 0));
            if (score > 0) results.Add(("📄 Document", d.Title, $"Status: {d.Status}, Ministry: {d.MinistryOrDepartment}", d.Oid, score));
        }

        // Search Rules
        var rules = await db.Rules.Include(r => r.DraftDocument).AsNoTracking().ToListAsync();
        foreach (var r in rules)
        {
            int score = keywords.Sum(k =>
                (r.SectionTitle.Contains(k, StringComparison.OrdinalIgnoreCase) ? 3 : 0) +
                (r.ProposedProvision.Contains(k, StringComparison.OrdinalIgnoreCase) ? 2 : 0) +
                (r.RuleNumber.Contains(k, StringComparison.OrdinalIgnoreCase) ? 4 : 0) +
                (r.ExistingProvision.Contains(k, StringComparison.OrdinalIgnoreCase) ? 1 : 0));
            if (score > 0) results.Add(("📋 Rule", $"Section {r.RuleNumber}: {r.SectionTitle}", Truncate(r.ProposedProvision, 120), r.Oid, score));
        }

        // Search Opinions
        var opinions = await db.Opinions.Include(o => o.Rule).AsNoTracking().Take(200).ToListAsync();
        foreach (var o in opinions)
        {
            int score = keywords.Sum(k =>
                (o.OpinionText.Contains(k, StringComparison.OrdinalIgnoreCase) ? 2 : 0) +
                ((o.Suggestion ?? "").Contains(k, StringComparison.OrdinalIgnoreCase) ? 1 : 0));
            if (score > 0) results.Add(("💬 Opinion", $"Feedback on {o.Rule?.SectionTitle ?? "a rule"}", Truncate(o.OpinionText, 100), o.Oid, score));
        }

        // Search Ministries
        var ministries = await db.Ministries.AsNoTracking().ToListAsync();
        foreach (var m in ministries)
        {
            int score = keywords.Sum(k =>
                (m.Name.Contains(k, StringComparison.OrdinalIgnoreCase) ? 3 : 0) +
                (m.Description.Contains(k, StringComparison.OrdinalIgnoreCase) ? 1 : 0));
            if (score > 0) results.Add(("🏛️ Ministry", m.Name, m.Description, m.Oid, score));
        }

        if (!results.Any())
        {
            return new ChatbotAnswerDto
            {
                Answer = $"I searched the entire DPCS database for \"{question}\" but couldn't find matching results.\n\n" +
                         "💡 **Try these tips:**\n" +
                         "• Use specific keywords (e.g., \"digital security\", \"privacy\")\n" +
                         "• Ask about a topic (e.g., \"cybercrime\", \"data protection\")\n" +
                         "• Ask general questions (e.g., \"what documents are active?\")\n\n" +
                         "I can search across all documents, rules, opinions, and ministries!",
                Intent = "NoResults", Confidence = 0.3,
                SuggestedQuestions = new() { "Show system statistics", "What documents are available?", "What is DPCS?" }
            };
        }

        var top = results.OrderByDescending(r => r.score).Take(5).ToList();
        var lines = top.Select((r, i) => $"**{i + 1}. {r.type} — {r.title}**\n   {r.snippet}");

        return new ChatbotAnswerDto
        {
            Answer = $"🔍 I found **{results.Count}** result(s) matching your query. Here are the top matches:\n\n{string.Join("\n\n", lines)}",
            Sources = top.Select(r => new ChatSourceReference { EntityType = r.type, EntityId = r.id, Title = r.title, Snippet = Truncate(r.snippet, 60) }).ToList(),
            Intent = "UniversalSearch", Confidence = Math.Min((double)top.Max(r => r.score) / 5 + 0.3, 1.0),
            SuggestedQuestions = new() { "Show more details", "Show system statistics", "What consultations are active?" }
        };
    }

    // ── Helpers ────────────────────────────────────────────────────────
    private List<string> ExtractKeywords(string question)
    {
        var stopWords = new HashSet<string> { "the", "a", "an", "is", "are", "was", "were", "what", "which", "who", "how", "where", "when", "do", "does", "did", "can", "could", "will", "would", "should", "may", "might", "about", "for", "with", "from", "this", "that", "these", "those", "and", "or", "but", "not", "in", "on", "at", "to", "of", "by", "it", "its", "my", "your", "our", "their", "me", "us", "them", "i", "you", "we", "they", "he", "she", "has", "have", "had", "be", "been", "being", "tell", "show", "give", "get", "find", "search", "look", "please", "any" };
        return Regex.Split(question.ToLower(), @"\W+")
            .Where(w => w.Length > 2 && !stopWords.Contains(w))
            .Distinct()
            .ToList();
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }

    private async Task SaveConversation(ChatbotQuestionDto question, ChatbotAnswerDto answer)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.ChatbotConversations.Add(new ChatbotConversation
            {
                Oid = Guid.NewGuid(),
                UserId = question.UserId,
                SessionId = question.SessionId,
                UserMessage = question.Question,
                BotResponse = answer.Answer,
                Intent = answer.Intent,
                Confidence = answer.Confidence,
                CreatedDate = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch { /* Don't let persistence failures break the chat */ }
    }
}

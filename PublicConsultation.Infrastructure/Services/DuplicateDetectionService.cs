using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using PublicConsultation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PublicConsultation.Infrastructure.Services;

public class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly ApplicationDbContext _dbContext;

    public DuplicateDetectionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Opinion>> FindSimilarOpinionsAsync(string text, Guid ruleId, double threshold = 0.7)
    {
        if (string.IsNullOrWhiteSpace(text)) return new List<Opinion>();

        var existingOpinions = await _dbContext.Opinions
            .Where(o => o.RuleId == ruleId)
            .ToListAsync();

        var similar = existingOpinions
            .Select(o => new { Opinion = o, Similarity = CalculateSimilarity(text, o.OpinionText) })
            .Where(x => x.Similarity >= threshold)
            .OrderByDescending(x => x.Similarity)
            .Select(x => x.Opinion)
            .ToList();

        return similar;
    }

    private double CalculateSimilarity(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0;

        var words1 = s1.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
        var words2 = s2.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();

        var intersect = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return (double)intersect / union; // Jaccard Similarity index
    }
}

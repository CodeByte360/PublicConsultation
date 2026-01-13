using PublicConsultation.Core.Entities;

namespace PublicConsultation.Core.Interfaces;

public interface IDuplicateDetectionService
{
    Task<List<Opinion>> FindSimilarOpinionsAsync(string text, Guid ruleId, double threshold = 0.7);
}

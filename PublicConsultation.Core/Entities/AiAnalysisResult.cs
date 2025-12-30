using System;

namespace PublicConsultation.Core.Entities;

public class AiAnalysisResult : BaseEntity
{
    public Guid TargetId { get; set; } // ID of the Opinion or Document being analyzed
    public string TargetType { get; set; } = string.Empty; // "Opinion" or "Document"
    public string AnalysisType { get; set; } = string.Empty; // "Sentiment", "Summary", "Keywords"

    public string? ResultText { get; set; }
    public double? ConfidenceScore { get; set; }
    public string? MetadataJson { get; set; }
}

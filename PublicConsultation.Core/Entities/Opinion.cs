using System;
using System.ComponentModel.DataAnnotations;

namespace PublicConsultation.Core.Entities;

public class Opinion : BaseEntity
{

    [Key]
    public Guid Oid { get; set; }
    public Guid UserId { get; set; }
    public UserAccount? User { get; set; }

    public Guid RuleId { get; set; }
    public Rule? Rule { get; set; }

    public string OpinionText { get; set; } = string.Empty;
    public string? Suggestion { get; set; } // Optional: user's proposed rewrite

    // AI Analysis Fields (to be populated later)
    public string? Sentiment { get; set; }
    public string? Summary { get; set; }
}

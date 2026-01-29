#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PublicConsultation.Core.Entities;

public class Biometric : BaseEntity
{
    /// <summary>
    /// Primary key identifying the biometric data entry.
    /// </summary>      
    [Key]
    public Guid Oid { get; set; }

    /// <summary>
    ///  Biometric data for the left thumb of the client.
    /// </summary>
    public string? LeftThumb { get; set; }

    /// <summary>
    /// Biometric data for the left index finger of the client.
    /// </summary>
    public string? LeftIndex { get; set; }

    /// <summary>
    /// Biometric data for the right thumb of the client.
    /// </summary>
    public string? RightThumb { get; set; }

    /// <summary>
    /// Biometric data for the right index finger of the client.
    /// </summary>
    public string? RightIndex { get; set; }

    /// <summary>
    /// Reference to the associated client entity through a foreign key relationship.
    /// </summary>
    public Guid UserAccountId { get; set; }
    [ForeignKey("UserAccountId")]
    public virtual UserAccount UserAccount { get; set; } = null!;
}

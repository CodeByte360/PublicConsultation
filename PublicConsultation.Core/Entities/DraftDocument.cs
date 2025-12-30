using System;
using System.Collections.Generic;

namespace PublicConsultation.Core.Entities;

public class DraftDocument : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string MinistryOrDepartment { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FilePath { get; set; } = string.Empty; // Path to PDF/DOCX
    public string Status { get; set; } = "Draft"; // Draft, Published, Closed
    public DateTime ConsultationStartDate { get; set; }
    public DateTime ConsultationEndDate { get; set; }

    public ICollection<Rule> Rules { get; set; } = new List<Rule>();
}

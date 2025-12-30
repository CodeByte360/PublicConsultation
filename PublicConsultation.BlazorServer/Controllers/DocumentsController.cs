using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicConsultation.Infrastructure.Data;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PublicConsultation.BlazorServer.Controllers;

[Route("documents")]
public class DocumentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DocumentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("download/{id:guid}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var document = await _context.DraftDocuments.FirstOrDefaultAsync(d => d.Id == id);
        if (document == null)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(document.FilePath) || !System.IO.File.Exists(document.FilePath))
        {
            return NotFound("File not found on server.");
        }

        var fileName = Path.GetFileName(document.FilePath);
        var mimeType = "application/pdf"; // Defaulting to PDF as per requirement
        if (fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(document.FilePath);
        return File(bytes, mimeType, fileName);
    }
}

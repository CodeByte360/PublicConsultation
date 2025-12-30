using PublicConsultation.Core.Entities;
using PublicConsultation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PublicConsultation.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly string _uploadDirectory;

    public DocumentService()
    {
        _uploadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Uploads");
        if (!Directory.Exists(_uploadDirectory))
        {
            Directory.CreateDirectory(_uploadDirectory);
        }
    }

    public async Task<string> UploadDocumentAsync(Stream fileStream, string fileName)
    {
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(_uploadDirectory, uniqueFileName);

        await using var fs = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fs);

        return filePath;
    }

    public Task<List<Rule>> ParseDocumentAsync(string filePath)
    {
        // Stub implementation for MVP
        // In a real application, integration with PdfSharp or OpenXml would happen here
        var rules = new List<Rule>();

        // Simulating parsing by adding a dummy rule
        rules.Add(new Rule
        {
            RuleNumber = "1",
            SectionTitle = "Short Title and Commencement",
            ExistingProvision = "This Act may be called the Digital Security Act, 2018.",
            ProposedProvision = "This Act may be called the Cyber Security Act, 2023.",
            DisplayOrder = 1
        });

        return Task.FromResult(rules);
    }
}

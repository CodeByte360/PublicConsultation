using PublicConsultation.Core.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PublicConsultation.Core.Interfaces;

public interface IDocumentService
{
    Task<string> UploadDocumentAsync(Stream fileStream, string fileName);
    Task<List<Rule>> ParseDocumentAsync(string filePath);
}

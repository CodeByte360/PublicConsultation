using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PublicConsultation.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PublicConsultation.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadProfilePictureAsync(Stream fileStream, string fileName)
    {
        if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
        if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("File name cannot be empty", nameof(fileName));

        // Validation
        var maxFileSize = 5 * 1024 * 1024; // 5MB
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (Array.IndexOf(allowedExtensions, extension) < 0)
        {
            throw new InvalidOperationException("Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
        }

        // fileStream.Length might throw for some streams (like browser upload)
        // Size validation should be handled by the caller or OpenReadStream limit

        _logger.LogInformation("Starting profile picture upload for file: {FileName}", fileName);

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            // Fallback for some dev environments where WebRootPath might be null
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _logger.LogWarning("WebRootPath is null. Using fallback path: {FallbackPath}", webRootPath);
        }

        var uploadsFolder = Path.Combine(webRootPath, "images", "profiles");
        if (!Directory.Exists(uploadsFolder))
        {
            _logger.LogInformation("Creating uploads directory: {UploadsFolder}", uploadsFolder);
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        _logger.LogInformation("Saving file to: {FilePath}", filePath);

        await using var outputStream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream);

        _logger.LogInformation("File saved successfully.");

        return $"/images/profiles/{uniqueFileName}";
    }
}

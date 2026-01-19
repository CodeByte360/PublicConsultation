using System.IO;
using System.Threading.Tasks;

namespace PublicConsultation.Core.Interfaces;

public interface IFileService
{
    Task<string> UploadProfilePictureAsync(Stream fileStream, string fileName);
}

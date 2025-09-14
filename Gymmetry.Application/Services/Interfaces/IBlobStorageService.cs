using System.Threading.Tasks;
using System.IO;
using System;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(string container, string blobPath, byte[] content, string contentType);
        Task<string> UploadTextAsync(string container, string blobPath, string text, string contentType = "application/json");
        Task<bool> DeleteIfExistsAsync(string container, string blobPath);
    }
}

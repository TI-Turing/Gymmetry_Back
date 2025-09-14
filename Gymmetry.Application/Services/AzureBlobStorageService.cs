using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Gymmetry.Application.Services.Interfaces;

namespace Gymmetry.Application.Services
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _client;
        public AzureBlobStorageService(BlobServiceClient client)
        {
            _client = client;
        }
        public async Task<string> UploadAsync(string container, string blobPath, byte[] content, string contentType)
        {
            var c = _client.GetBlobContainerClient(container);
            await c.CreateIfNotExistsAsync();
            var blob = c.GetBlobClient(blobPath);
            using var ms = new MemoryStream(content);
            await blob.UploadAsync(ms, new BlobHttpHeaders{ ContentType = contentType });
            return blob.Uri.ToString();
        }
        public async Task<string> UploadTextAsync(string container, string blobPath, string text, string contentType = "application/json")
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            return await UploadAsync(container, blobPath, bytes, contentType);
        }
        public async Task<bool> DeleteIfExistsAsync(string container, string blobPath)
        {
            var c = _client.GetBlobContainerClient(container);
            var blob = c.GetBlobClient(blobPath);
            var resp = await blob.DeleteIfExistsAsync();
            return resp.Value;
        }
    }
}

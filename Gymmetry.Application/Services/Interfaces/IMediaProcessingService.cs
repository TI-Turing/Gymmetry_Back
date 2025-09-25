using System;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO.Feed.Request;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IMediaProcessingService
    {
        Task<ProcessedMediaFile> ProcessImageAsync(FileData file, MediaProcessingOptions options);
        Task<ProcessedMediaFile> ProcessVideoAsync(FileData file, MediaProcessingOptions options);
        Task<MediaValidationResult> ValidateMediaFileAsync(FileData file);
        Task<(int width, int height)> GetImageDimensionsAsync(FileData file);
        Task<(int width, int height, int durationSeconds)> GetVideoInfoAsync(FileData file);
    }

    public class ProcessedMediaFile
    {
        public byte[] Data { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string OriginalFileName { get; set; } = null!;
        public long SizeBytes { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? DurationSeconds { get; set; } // Solo para videos
        public string FileExtension { get; set; } = null!;
    }

    public class MediaProcessingOptions
    {
        public int MaxWidth { get; set; } = 1920;
        public int MaxHeight { get; set; } = 1080;
        public int JpegQuality { get; set; } = 85;
        public long MaxSizeBytes { get; set; } = 50 * 1024 * 1024; // 50MB
        public int MaxDurationSeconds { get; set; } = 180; // 3 minutos
        public int TargetBitrate { get; set; } = 1500000; // 1.5 Mbps
    }

    public class MediaValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string MediaType { get; set; } = null!; // "image" or "video"
        public string ContentType { get; set; } = null!;
    }
}
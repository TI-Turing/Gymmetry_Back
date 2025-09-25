using System;

namespace Gymmetry.Domain.Models
{
    public class FeedMedia
    {
        public Guid Id { get; set; }
        public Guid FeedId { get; set; }
        public string MediaUrl { get; set; } = null!;
        public string MediaType { get; set; } = null!;
        public string? FileName { get; set; }
        public long FileSizeBytes { get; set; }
        public string BlobName { get; set; } = null!;
        public int? DurationSeconds { get; set; } // Para videos
        public int? Width { get; set; } // Para imágenes y videos
        public int? Height { get; set; } // Para imágenes y videos
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation property
        public virtual Feed Feed { get; set; } = null!;
    }
}
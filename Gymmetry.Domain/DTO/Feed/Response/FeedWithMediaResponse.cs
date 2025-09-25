using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.Feed.Response
{
    public class FeedWithMediaResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Description { get; set; } = null!;
        public bool IsAnonymous { get; set; }
        public string? Hashtags { get; set; }
        public List<MediaFileInfo> MediaFiles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
    }

    public class MediaFileInfo
    {
        public string Url { get; set; } = null!;
        public string MediaType { get; set; } = null!;
        public string? FileName { get; set; }
        public long SizeBytes { get; set; }
        public int? DurationSeconds { get; set; } // Para videos
        public int? Width { get; set; } // Para imágenes y videos
        public int? Height { get; set; } // Para imágenes y videos
    }
}
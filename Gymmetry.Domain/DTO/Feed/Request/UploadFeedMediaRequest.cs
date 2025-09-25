using System;

namespace Gymmetry.Domain.DTO.Feed.Request
{
    public class UploadFeedMediaRequest : ApiRequest
    {
        public Guid FeedId { get; set; }
        public byte[] Media { get; set; } = null!;
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }
}
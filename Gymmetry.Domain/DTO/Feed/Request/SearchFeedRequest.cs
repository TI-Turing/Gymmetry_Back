using System;

namespace Gymmetry.Domain.DTO.Feed.Request
{
    public class SearchFeedRequest : ApiRequest
    {
        // Puedes buscar por título, descripción, usuario, hashtags, etc.
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Guid? UserId { get; set; }
        public string? Hashtag { get; set; }
    }
}
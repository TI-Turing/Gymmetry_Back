using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.ContentModeration
{
    public class ContentModerationListResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<ContentModerationResponse> Items { get; set; } = new();
    }
}
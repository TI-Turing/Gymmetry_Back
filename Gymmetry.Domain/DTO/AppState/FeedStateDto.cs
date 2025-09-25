using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.AppState
{
    public class FeedStateDto
    {
        public List<Gymmetry.Domain.Models.Feed> RecentFeeds { get; set; } = new();
        public List<Gymmetry.Domain.Models.Feed> TrendingFeeds { get; set; } = new();
        public int TotalFeedCount { get; set; }
    }
}
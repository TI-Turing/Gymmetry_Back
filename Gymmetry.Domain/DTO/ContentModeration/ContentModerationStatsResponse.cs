using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.ContentModeration
{
    public class ContentModerationStatsResponse
    {
        public int TotalModerations { get; set; }
        public int AutoModerations { get; set; }
        public int ManualModerations { get; set; }
        public int PendingReviews { get; set; }
        public Dictionary<string, int> ByAction { get; set; } = new();
        public Dictionary<string, int> ByReason { get; set; } = new();
        public Dictionary<string, int> BySeverity { get; set; } = new();
        public Dictionary<string, int> ByContentType { get; set; } = new();
        public Dictionary<string, int> Last7Days { get; set; } = new();
    }
}
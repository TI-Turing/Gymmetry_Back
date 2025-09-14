using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.ReportContent
{
    public class ReportContentStatsResponse
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int UnderReview { get; set; }
        public int Resolved { get; set; }
        public int Dismissed { get; set; }
        public Dictionary<string,int> ByReason { get; set; } = new();
        public Dictionary<string,int> ByPriority { get; set; } = new();
        public Dictionary<string,int> ByContentType { get; set; } = new();
    }
}

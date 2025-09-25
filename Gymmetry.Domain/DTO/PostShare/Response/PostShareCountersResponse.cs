using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.PostShare.Response
{
    public class PostShareCountersResponse
    {
        public Guid PostId { get; set; }
        public int TotalShares { get; set; }
        public int InternalShares { get; set; }
        public int ExternalShares { get; set; }
        public Dictionary<string, int> ByPlatform { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }
}
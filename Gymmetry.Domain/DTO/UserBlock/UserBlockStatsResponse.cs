using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.UserBlock
{
    public class UserBlockStatsResponse
    {
        public int TotalBlocks { get; set; }
        public int BlockedByMe { get; set; }
        public int BlockingMe { get; set; }
        public int MutualBlocks { get; set; }
        public int TodayBlocks { get; set; }
        public Dictionary<string, int> BlocksByDay { get; set; } = new();
    }
}
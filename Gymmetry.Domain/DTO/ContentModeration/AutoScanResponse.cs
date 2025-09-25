using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.ContentModeration
{
    public class AutoScanResponse
    {
        public int ItemsScanned { get; set; }
        public int ItemsFlagged { get; set; }
        public int ItemsModerated { get; set; }
        public Dictionary<string, int> FilterResults { get; set; } = new();
    }
}
using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.UserBlock
{
    public class BulkUnblockResponse
    {
        public int TotalRequested { get; set; }
        public int SuccessfullyUnblocked { get; set; }
        public List<Guid> FailedUnblocks { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
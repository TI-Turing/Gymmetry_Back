using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.ContentModeration
{
    public class BulkModerationResponse
    {
        public int TotalRequested { get; set; }
        public int SuccessfullyProcessed { get; set; }
        public List<Guid> FailedIds { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
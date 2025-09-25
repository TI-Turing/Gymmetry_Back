using System;

namespace Gymmetry.Domain.DTO.UserBlock
{
    public class UserBlockCheckResponse
    {
        public bool IsBlocked { get; set; }
        public bool IsMutual { get; set; }
        public DateTime? BlockedAt { get; set; }
        public string? Reason { get; set; }
    }
}
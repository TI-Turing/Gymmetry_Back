using System;

namespace Gymmetry.Domain.DTO.UserBlock
{
    public class UserBlockResponse
    {
        public Guid Id { get; set; }
        public Guid BlockerId { get; set; }
        public Guid BlockedUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Reason { get; set; }
        public string? BlockerName { get; set; }
        public string? BlockedUserName { get; set; }
    }
}
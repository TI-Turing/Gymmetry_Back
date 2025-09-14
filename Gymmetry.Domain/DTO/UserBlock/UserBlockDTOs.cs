using System;
using System.ComponentModel.DataAnnotations;
using Gymmetry.Domain.DTO;

namespace Gymmetry.Domain.DTO.UserBlock
{
    public class UserBlockCreateRequest : ApiRequest
    {
        [Required]
        public Guid BlockedUserId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }

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

    public class UserBlockListResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<UserBlockResponse> Items { get; set; } = new();
    }

    public class UserBlockStatsResponse
    {
        public int TotalBlocks { get; set; }
        public int BlockedByMe { get; set; }
        public int BlockingMe { get; set; }
        public int MutualBlocks { get; set; }
        public int TodayBlocks { get; set; }
        public Dictionary<string, int> BlocksByDay { get; set; } = new();
    }

    public class UserBlockCheckResponse
    {
        public bool IsBlocked { get; set; }
        public bool IsMutual { get; set; }
        public DateTime? BlockedAt { get; set; }
        public string? Reason { get; set; }
    }

    public class BulkUnblockRequest : ApiRequest
    {
        [Required]
        public List<Guid> UserIds { get; set; } = new();
    }

    public class BulkUnblockResponse
    {
        public int TotalRequested { get; set; }
        public int SuccessfullyUnblocked { get; set; }
        public List<Guid> FailedUnblocks { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
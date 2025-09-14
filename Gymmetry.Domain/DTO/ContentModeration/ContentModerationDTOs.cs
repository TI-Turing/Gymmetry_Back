using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;

namespace Gymmetry.Domain.DTO.ContentModeration
{
    public class ContentModerationCreateRequest : ApiRequest
    {
        [Required]
        public Guid ContentId { get; set; }

        [Required]
        public ContentType ContentType { get; set; }

        [Required]
        public ModerationAction ModerationAction { get; set; }

        [Required]
        public ModerationReason ModerationReason { get; set; }

        [Required]
        public ModerationSeverity Severity { get; set; }

        public Guid? ModeratedBy { get; set; }

        public bool AutoModerated { get; set; } = false;

        public bool ReviewRequired { get; set; } = false;

        [MaxLength(100)]
        public string? FilterType { get; set; }

        [Range(0, 1)]
        public decimal? Confidence { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ContentModerationUpdateRequest : ApiRequest
    {
        [Required]
        public Guid Id { get; set; }

        public ModerationAction? ModerationAction { get; set; }

        public ModerationSeverity? Severity { get; set; }

        public Guid? ModeratedBy { get; set; }

        public bool? ReviewRequired { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class ContentModerationResponse
    {
        public Guid Id { get; set; }
        public Guid ContentId { get; set; }
        public ContentType ContentType { get; set; }
        public ModerationAction ModerationAction { get; set; }
        public ModerationReason ModerationReason { get; set; }
        public ModerationSeverity Severity { get; set; }
        public Guid? ModeratedBy { get; set; }
        public DateTime ModeratedAt { get; set; }
        public bool AutoModerated { get; set; }
        public bool ReviewRequired { get; set; }
        public string? FilterType { get; set; }
        public decimal? Confidence { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ModeratorName { get; set; }
    }

    public class ContentModerationListResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<ContentModerationResponse> Items { get; set; } = new();
    }

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

    public class BulkModerationRequest : ApiRequest
    {
        [Required]
        public List<Guid> ModerationIds { get; set; } = new();

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class BulkModerationResponse
    {
        public int TotalRequested { get; set; }
        public int SuccessfullyProcessed { get; set; }
        public List<Guid> FailedIds { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    public class AutoScanRequest : ApiRequest
    {
        public ContentType? ContentType { get; set; }
        public DateTime? SinceDate { get; set; }
        public int? LimitItems { get; set; } = 100;
    }

    public class AutoScanResponse
    {
        public int ItemsScanned { get; set; }
        public int ItemsFlagged { get; set; }
        public int ItemsModerated { get; set; }
        public Dictionary<string, int> FilterResults { get; set; } = new();
    }
}
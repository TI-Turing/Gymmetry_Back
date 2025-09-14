using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.Models
{
    public enum ContentType
    {
        Feed = 1,
        Comment = 2
    }

    public enum ModerationAction
    {
        NoAction = 0,
        Warning = 1,
        Hidden = 2,
        Removed = 3,
        Flagged = 4
    }

    public enum ModerationReason
    {
        AutoFilter = 1,
        ManualReview = 2,
        UserReport = 3,
        AIDetection = 4,
        CommunityFlag = 5
    }

    public enum ModerationSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    public class ContentModeration
    {
        [Key]
        public Guid Id { get; set; }

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

        [Required]
        public DateTime ModeratedAt { get; set; }

        [Required]
        public bool AutoModerated { get; set; }

        [Required]
        public bool ReviewRequired { get; set; }

        [MaxLength(100)]
        public string? FilterType { get; set; }

        public decimal? Confidence { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(45)]
        public string? Ip { get; set; }

        // Navigation properties
        public virtual User? Moderator { get; set; }
    }
}
using System;

namespace Gymmetry.Domain.Models
{
    public enum ReportContentType { Feed = 1, Comment = 2 }
    public enum ReportReason { Spam = 1, Harassment, InappropriateContent, Hate, Violence, Misinformation, Other }
    public enum ReportStatus { Pending = 1, UnderReview, Resolved, Dismissed }
    public enum ReportPriority { Low = 1, Medium, High, Critical }

    public class ReportContent
    {
        public Guid Id { get; set; }
        public Guid ReportedContentId { get; set; }
        public ReportContentType ContentType { get; set; }
        public Guid ReporterId { get; set; }
        public Guid ReportedUserId { get; set; }
        public ReportReason Reason { get; set; }
        public string? Description { get; set; }
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public ReportPriority Priority { get; set; } = ReportPriority.Medium;
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? Resolution { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }

        public User? Reporter { get; set; }
        public User? ReportedUser { get; set; }
        public User? Reviewer { get; set; }
    }
}

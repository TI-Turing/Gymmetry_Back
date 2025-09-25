using System;
using Gymmetry.Domain.Models;

namespace Gymmetry.Domain.DTO.ContentModeration
{
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
}
using System;
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
}
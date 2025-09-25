using System;
using System.ComponentModel.DataAnnotations;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;

namespace Gymmetry.Domain.DTO.ContentModeration
{
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
}
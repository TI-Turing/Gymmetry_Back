using System;
using System.ComponentModel.DataAnnotations;
using Gymmetry.Domain.Models;

namespace Gymmetry.Domain.DTO.ReportContent
{
    public class ReportContentCreateRequest : ApiRequest
    {
        [Required]
        public Guid ReportedContentId { get; set; }
        [Required]
        public ReportContentType ContentType { get; set; }
        [Required]
        public Guid ReporterId { get; set; }
        [Required]
        public Guid ReportedUserId { get; set; }
        [Required]
        public ReportReason Reason { get; set; }
        public string? Description { get; set; }
        public ReportPriority Priority { get; set; } = ReportPriority.Medium;
    }
}

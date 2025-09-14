using System;
using Gymmetry.Domain.Models;

namespace Gymmetry.Domain.DTO.ReportContent
{
    public class ReportContentResponse
    {
        public Guid Id { get; set; }
        public Guid ReportedContentId { get; set; }
        public ReportContentType ContentType { get; set; }
        public Guid ReporterId { get; set; }
        public Guid ReportedUserId { get; set; }
        public ReportReason Reason { get; set; }
        public string? Description { get; set; }
        public ReportStatus Status { get; set; }
        public ReportPriority Priority { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? Resolution { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

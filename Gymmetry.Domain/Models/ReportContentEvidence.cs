using System;

namespace Gymmetry.Domain.Models
{
    public class ReportContentEvidence
    {
        public Guid Id { get; set; }
        public Guid ReportContentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty; // could be URL or blob path
        public long SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ReportContent? Report { get; set; }
    }
}

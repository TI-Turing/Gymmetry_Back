using System;

namespace Gymmetry.Domain.Models
{
    public class ReportContentAudit
    {
        public Guid Id { get; set; }
        public Guid ReportContentId { get; set; }
        public string Action { get; set; } = string.Empty; // created, updated, reviewed, resolved, dismissed
        public string SnapshotJson { get; set; } = string.Empty; // serialized snapshot
        public Guid? ActorUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Ip { get; set; }
    }
}

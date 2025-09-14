using System;
using System.ComponentModel.DataAnnotations;
using Gymmetry.Domain.Models;

namespace Gymmetry.Domain.DTO.ReportContent
{
    public class ReportContentUpdateRequest : ApiRequest
    {
        [Required]
        public Guid Id { get; set; }
        public ReportStatus Status { get; set; }
        public ReportPriority Priority { get; set; }
        public Guid? ReviewedBy { get; set; }
        public string? Resolution { get; set; }
    }
}

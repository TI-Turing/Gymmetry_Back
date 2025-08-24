using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.Analytics
{
    public class AnalyticsSummaryRequest
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string StartDate { get; set; } = default!; // YYYY-MM-DD
        [Required]
        public string EndDate { get; set; } = default!;   // YYYY-MM-DD (inclusive)
        public string? Timezone { get; set; }
    }
}

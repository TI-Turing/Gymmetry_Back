using System;using System.Collections.Generic;using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.Progress
{
    public class PeriodRange
    {
        [Required] public string From { get; set; } = default!; // yyyy-MM-dd
        [Required] public string To { get; set; } = default!;   // yyyy-MM-dd
    }

    public class MultiProgressReportRequest
    {
        public Guid? UserId { get; set; }
        public string? Timezone { get; set; }
        [Required] public List<PeriodRange> Periods { get; set; } = new();
        public bool IncludeAssessments { get; set; } = true;
        public bool ComparePreviousPeriod { get; set; } = false; // se aplica a cada periodo individual
        public int MinCompletionForAdherence { get; set; } = 30;
        public int TopExercises { get; set; } = 10;
        public bool IncludeHistory { get; set; } = false;
    }
}

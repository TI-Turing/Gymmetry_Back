using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.Progress
{
    public class ProgressReportRequest
    {
        // UserId ahora opcional; si viene null o Guid.Empty se tomar� del JWT en las Functions
        public Guid? UserId { get; set; }
        [Required]
        public string StartDate { get; set; } = default!; // yyyy-MM-dd
        [Required]
        public string EndDate { get; set; } = default!;   // yyyy-MM-dd inclusive
        public string? Timezone { get; set; }
        public bool IncludeAssessments { get; set; } = true;
        public bool ComparePreviousPeriod { get; set; } = false;
        public int MinCompletionForAdherence { get; set; } = 30; // porcentaje m�nimo para marcar d�a cumplido
        public int TopExercises { get; set; } = 10; // l�mite de ranking
    }
}

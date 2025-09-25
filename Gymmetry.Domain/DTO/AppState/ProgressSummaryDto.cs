using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.AppState
{
    public class ProgressSummaryDto
    {
        public decimal AdherencePercentage { get; set; }
        public int WorkoutsSummary { get; set; }
        public int TotalMinutes { get; set; }
        public Dictionary<string, decimal> MuscleDistribution { get; set; } = new();
        public List<string> DominantMuscles { get; set; } = new();
        public List<string> UnderworkedMuscles { get; set; } = new();
        public decimal BalanceIndex { get; set; }
    }
}
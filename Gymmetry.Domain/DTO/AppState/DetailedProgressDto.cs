using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.AppState
{
    public class DetailedProgressDto
    {
        public int TotalWorkouts { get; set; }
        public int TotalMinutes { get; set; }
        public int AvgWorkoutMinutes { get; set; }
        public decimal AvgCompletionRate { get; set; }
        public List<RecentWorkoutDto> RecentWorkouts { get; set; } = new();
    }
}
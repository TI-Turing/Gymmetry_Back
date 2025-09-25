using System;

namespace Gymmetry.Domain.DTO.AppState
{
    public class RecentWorkoutDto
    {
        public DateTime Date { get; set; }
        public int DurationMinutes { get; set; }
        public int CompletionPercentage { get; set; }
        public string? BranchName { get; set; }
    }
}
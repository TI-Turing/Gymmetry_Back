using System;

namespace Gymmetry.Domain.DTO.AppState
{
    public class ProfileStatsDto
    {
        public int TotalWorkouts { get; set; }
        public int CurrentStreak { get; set; }
        public int TotalDays { get; set; }
        public DateTime? MemberSince { get; set; }
        public string? CurrentWeight { get; set; }
        public string? CurrentHeight { get; set; }
        public DateTime? LastAssessment { get; set; }
    }
}
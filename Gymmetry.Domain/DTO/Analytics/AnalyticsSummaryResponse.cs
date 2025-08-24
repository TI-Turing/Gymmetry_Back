using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.Analytics
{
    public class AnalyticsSummaryResponse
    {
        public int TotalWorkouts { get; set; }
        public decimal TotalCalories { get; set; }
        public int TotalDurationMinutes { get; set; }
        public int AvgDurationMinutes { get; set; }
        public int DaysAdvanced { get; set; }
        public int DaysExpected { get; set; }
        public int CurrentStreakDays { get; set; }
        public int LongestStreakDays { get; set; }
        public decimal? CurrentWeightKg { get; set; }
        public decimal? WeightChangeKg { get; set; }
        public List<RoutineUsageItem> RoutineUsage { get; set; } = new();
        public List<WeekdayDisciplineItem> WeekdayDiscipline { get; set; } = new();
        public List<BranchAttendanceItem> BranchAttendance { get; set; } = new();
        public List<DailySeriesItem>? DailySeries { get; set; }
        public string GeneratedAt { get; set; } = DateTime.UtcNow.ToString("O");
    }
    public class RoutineUsageItem
    {
        public Guid RoutineTemplateId { get; set; }
        public string RoutineTemplateName { get; set; } = string.Empty;
        public int DaysUsed { get; set; }
        public int DaysExpected { get; set; }
        public decimal UsagePercent { get; set; }
    }
    public class WeekdayDisciplineItem
    {
        public int Weekday { get; set; }
        public int DaysAdvanced { get; set; }
        public int DaysExpected { get; set; }
    }
    public class BranchAttendanceItem
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int Visits { get; set; }
        public decimal Percent { get; set; }
    }
    public class DailySeriesItem
    {
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD
        public bool Advanced { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Calories { get; set; }
    }
}

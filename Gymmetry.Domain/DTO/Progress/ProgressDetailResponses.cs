using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.Progress
{
    public class ExercisesDetailResponse
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public int DistinctExercises { get; set; }
        public List<ExerciseFreq> Exercises { get; set; } = new();
        public int TotalSeries { get; set; }
        public int TotalReps { get; set; }
    }

    public class ObjectivesDetailResponse
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public Dictionary<string, decimal> Planned { get; set; } = new();
        public Dictionary<string, decimal> Executed { get; set; } = new();
        public List<ObjectiveGap> Gaps { get; set; } = new();
    }

    public class MusclesDetailResponse
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public Dictionary<string, decimal> Distribution { get; set; } = new();
        public List<string> Dominant { get; set; } = new();
        public List<string> Underworked { get; set; } = new();
        public decimal BalanceIndex { get; set; }
        public List<string> Alerts { get; set; } = new();
    }

    public class SuggestionsResponse
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
    }

    public class DisciplineDetailResponse
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public decimal ConsistencyIndex { get; set; }
        public string? CommonStartHour { get; set; }
        public decimal ScheduleRegularity { get; set; }
        public List<WeekdayBreakdown> Weekdays { get; set; } = new();
        public int CurrentStreak { get; set; }
        public int MaxStreak { get; set; }
        public decimal AdherencePct { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.Progress
{
    public class ProgressSummaryResponse
    {
        public PeriodInfo Period { get; set; } = new();
        public AdherenceBlock Adherence { get; set; } = new();
        public ExecutionBlock Execution { get; set; } = new();
        public TimeBlock Time { get; set; } = new();
        public ExerciseBlock Exercises { get; set; } = new();
        public ObjectiveBlock Objectives { get; set; } = new();
        public MuscleBlock Muscles { get; set; } = new();
        public ComparisonBlock? Comparison { get; set; }
        public AssessmentBlock? Assessments { get; set; }
        public DisciplineBlock Discipline { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public string GeneratedAt { get; set; } = DateTime.UtcNow.ToString("O");
    }

    public class PeriodInfo { public string From { get; set; } = string.Empty; public string To { get; set; } = string.Empty; public int Days { get; set; } }

    public class AdherenceBlock
    {
        public int TargetDays { get; set; }
        public int Sessions { get; set; }
        public int CompletedDays { get; set; }
        public decimal AdherencePct { get; set; }
        public int CurrentStreak { get; set; }
        public int MaxStreak { get; set; }
        public List<WeekdayBreakdown> ByWeekday { get; set; } = new();
        public List<BranchUsage> BranchAttendance { get; set; } = new();
    }

    public class WeekdayBreakdown { public int Weekday { get; set; } public int Done { get; set; } public int Expected { get; set; } }
    public class BranchUsage { public Guid BranchId { get; set; } public string Name { get; set; } = string.Empty; public int Visits { get; set; } public decimal Percent { get; set; } }

    public class ExecutionBlock
    {
        public decimal AvgCompletion { get; set; }
        public decimal StdevCompletion { get; set; }
        public List<SessionInfo> BestSessions { get; set; } = new();
        public List<SessionInfo> LowCompletionSessions { get; set; } = new();
        public List<DailyPoint> Series { get; set; } = new();
    }
    public class SessionInfo { public Guid DailyId { get; set; } public string Date { get; set; } = string.Empty; public int Percentage { get; set; } public int DurationMinutes { get; set; } }
    public class DailyPoint { public string Date { get; set; } = string.Empty; public int DurationMinutes { get; set; } public int Percentage { get; set; } }

    public class TimeBlock { public int TotalMinutes { get; set; } public int AvgPerSession { get; set; } public int MinSession { get; set; } public int MaxSession { get; set; } }

    public class ExerciseBlock
    {
        public int DistinctExercises { get; set; }
        public List<ExerciseFreq> TopExercises { get; set; } = new();
        public List<ExerciseFreq> UnderusedExercises { get; set; } = new();
        public int TotalSeries { get; set; }
        public int TotalReps { get; set; }
        public List<MissingPlannedItem> MissingPlanned { get; set; } = new();
        public List<ExerciseFreq> NewExercises { get; set; } = new();
    }
    public class ExerciseFreq { public Guid ExerciseId { get; set; } public string Name { get; set; } = string.Empty; public int Sessions { get; set; } public int Series { get; set; } public int Reps { get; set; } public decimal PercentSessions { get; set; } }
    public class MissingPlannedItem { public Guid ExerciseId { get; set; } public string Name { get; set; } = string.Empty; public int PlannedOccurrences { get; set; } public int ExecutedOccurrences { get; set; } }

    public class ObjectiveBlock
    {
        public Dictionary<string, decimal> Planned { get; set; } = new();
        public Dictionary<string, decimal> Executed { get; set; } = new();
        public List<ObjectiveGap> Gaps { get; set; } = new();
    }
    public class ObjectiveGap { public string Objective { get; set; } = string.Empty; public decimal Planned { get; set; } public decimal Executed { get; set; } public decimal Gap { get; set; } }

    public class MuscleBlock
    {
        public Dictionary<string, decimal> Distribution { get; set; } = new();
        public List<string> Dominant { get; set; } = new();
        public List<string> Underworked { get; set; } = new();
        public decimal BalanceIndex { get; set; }
        public List<string> Alerts { get; set; } = new();
    }

    public class ComparisonBlock
    {
        public SimpleWindowStats FirstHalf { get; set; } = new();
        public SimpleWindowStats SecondHalf { get; set; } = new();
        public string Trend { get; set; } = string.Empty;
    }
    public class SimpleWindowStats { public decimal AvgCompletion { get; set; } public int Sessions { get; set; } public int TotalMinutes { get; set; } public int DistinctExercises { get; set; } }

    public class AssessmentBlock
    {
        public Dictionary<string, string?> Latest { get; set; } = new();
        public List<AssessmentChange> Changes { get; set; } = new();
    }
    public class AssessmentChange { public string Field { get; set; } = string.Empty; public string? OldValue { get; set; } public string? NewValue { get; set; } }

    public class DisciplineBlock { public decimal ConsistencyIndex { get; set; } public string? CommonStartHour { get; set; } public decimal ScheduleRegularity { get; set; } }
}

using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.AppState
{
    public class TodayRoutineDto
    {
        public bool HasTrainedToday { get; set; }
        public Guid? TodayRoutineDayId { get; set; }
        public string? RoutineName { get; set; }
        public int? EstimatedDurationMinutes { get; set; }
        public List<string> TodayExercises { get; set; } = new();
        public DateTime? LastWorkout { get; set; }
    }
}
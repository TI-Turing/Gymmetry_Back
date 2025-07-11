using System;

namespace FitGymApp.Domain.DTO.DailyExerciseHistory.Request
{
    public class AddDailyExerciseHistoryRequest
    {
        public string Set { get; set; } = null!;
        public string Repetitions { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid DailyHistoryId { get; set; }
    }
}

using System;

namespace FitGymApp.Domain.DTO.DailyExerciseHistory.Request
{
    public class UpdateDailyExerciseHistoryRequest
    {
        public Guid Id { get; set; }
        public string Set { get; set; } = null!;
        public string Repetitions { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid DailyHistoryId { get; set; }
    }
}

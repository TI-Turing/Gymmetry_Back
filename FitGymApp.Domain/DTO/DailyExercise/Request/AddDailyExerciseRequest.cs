using System;

namespace FitGymApp.Domain.DTO.DailyExercise.Request
{
    public class AddDailyExerciseRequest
    {
        public string Set { get; set; } = null!;
        public string Repetitions { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid DailyId { get; set; }
        public Guid ExerciseId { get; set; }
    }
}

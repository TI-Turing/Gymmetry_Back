using System;

namespace FitGymApp.Domain.DTO.DailyExercise.Request
{
    public class UpdateDailyExerciseRequest
    {
        public Guid Id { get; set; }
        public string Set { get; set; } = null!;
        public string Repetitions { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid DailyId { get; set; }
        public Guid ExerciseId { get; set; }
    }
}

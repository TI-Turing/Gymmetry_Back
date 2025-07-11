using System;

namespace FitGymApp.Domain.DTO.RoutineExercise.Request
{
    public class UpdateRoutineExerciseRequest
    {
        public Guid Id { get; set; }
        public string Sets { get; set; } = null!;
        public string Repetitions { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid RoutineTemplateId { get; set; }
        public Guid ExerciseId { get; set; }
    }
}

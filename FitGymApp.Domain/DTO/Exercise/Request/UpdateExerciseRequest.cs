using System;

namespace FitGymApp.Domain.DTO.Exercise.Request
{
    public class UpdateExerciseRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid CategoryExerciseId { get; set; }
        public Guid CategoryExerciseId1 { get; set; }
    }
}

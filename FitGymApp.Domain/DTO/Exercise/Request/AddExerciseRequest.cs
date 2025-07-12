using System;

namespace FitGymApp.Domain.DTO.Exercise.Request
{
    public class AddExerciseRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
        public Guid CategoryExerciseId { get; set; }
        public Guid CategoryExerciseId1 { get; set; }
    }
}

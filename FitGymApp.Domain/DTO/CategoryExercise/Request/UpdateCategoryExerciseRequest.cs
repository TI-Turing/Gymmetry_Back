using System;

namespace FitGymApp.Domain.DTO.CategoryExercise.Request
{
    public class UpdateCategoryExerciseRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}

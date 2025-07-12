using System;

namespace FitGymApp.Domain.DTO.CategoryExercise.Request
{
    public class AddCategoryExerciseRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
    }
}

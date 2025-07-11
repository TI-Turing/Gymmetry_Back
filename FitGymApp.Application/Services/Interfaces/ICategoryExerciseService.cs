using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.CategoryExercise.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface ICategoryExerciseService
    {
        ApplicationResponse<CategoryExercise> CreateCategoryExercise(AddCategoryExerciseRequest request);
        ApplicationResponse<CategoryExercise> GetCategoryExerciseById(Guid id);
        ApplicationResponse<IEnumerable<CategoryExercise>> GetAllCategoryExercises();
        ApplicationResponse<bool> UpdateCategoryExercise(UpdateCategoryExerciseRequest request);
        ApplicationResponse<bool> DeleteCategoryExercise(Guid id);
        ApplicationResponse<IEnumerable<CategoryExercise>> FindCategoryExercisesByFields(Dictionary<string, object> filters);
    }
}

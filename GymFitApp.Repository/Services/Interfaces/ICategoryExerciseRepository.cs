using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface ICategoryExerciseRepository
    {
        Task<CategoryExercise> CreateCategoryExerciseAsync(CategoryExercise entity);
        Task<CategoryExercise?> GetCategoryExerciseByIdAsync(Guid id);
        Task<IEnumerable<CategoryExercise>> GetAllCategoryExercisesAsync();
        Task<bool> UpdateCategoryExerciseAsync(CategoryExercise entity);
        Task<bool> DeleteCategoryExerciseAsync(Guid id);
        Task<IEnumerable<CategoryExercise>> FindCategoryExercisesByFieldsAsync(Dictionary<string, object> filters);
    }
}

using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface ICategoryExerciseRepository
    {
        CategoryExercise CreateCategoryExercise(CategoryExercise entity);
        CategoryExercise GetCategoryExerciseById(Guid id);
        IEnumerable<CategoryExercise> GetAllCategoryExercises();
        bool UpdateCategoryExercise(CategoryExercise entity);
        bool DeleteCategoryExercise(Guid id);
        IEnumerable<CategoryExercise> FindCategoryExercisesByFields(Dictionary<string, object> filters);
    }
}

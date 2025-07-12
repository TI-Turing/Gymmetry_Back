using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IExerciseRepository
    {
        Task<Exercise> CreateExerciseAsync(Exercise entity);
        Task<Exercise?> GetExerciseByIdAsync(Guid id);
        Task<IEnumerable<Exercise>> GetAllExercisesAsync();
        Task<bool> UpdateExerciseAsync(Exercise entity);
        Task<bool> DeleteExerciseAsync(Guid id);
        Task<IEnumerable<Exercise>> FindExercisesByFieldsAsync(Dictionary<string, object> filters);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IDailyExerciseHistoryRepository
    {
        Task<DailyExerciseHistory> CreateDailyExerciseHistoryAsync(DailyExerciseHistory entity);
        Task<DailyExerciseHistory?> GetDailyExerciseHistoryByIdAsync(Guid id);
        Task<IEnumerable<DailyExerciseHistory>> GetAllDailyExerciseHistoriesAsync();
        Task<bool> UpdateDailyExerciseHistoryAsync(DailyExerciseHistory entity);
        Task<bool> DeleteDailyExerciseHistoryAsync(Guid id);
        Task<IEnumerable<DailyExerciseHistory>> FindDailyExerciseHistoriesByFieldsAsync(Dictionary<string, object> filters);
    }
}

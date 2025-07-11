using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IDailyExerciseHistoryRepository
    {
        DailyExerciseHistory CreateDailyExerciseHistory(DailyExerciseHistory entity);
        DailyExerciseHistory GetDailyExerciseHistoryById(Guid id);
        IEnumerable<DailyExerciseHistory> GetAllDailyExerciseHistories();
        bool UpdateDailyExerciseHistory(DailyExerciseHistory entity);
        bool DeleteDailyExerciseHistory(Guid id);
        IEnumerable<DailyExerciseHistory> FindDailyExerciseHistoriesByFields(Dictionary<string, object> filters);
    }
}

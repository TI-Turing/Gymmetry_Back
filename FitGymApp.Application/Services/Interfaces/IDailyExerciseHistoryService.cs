using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.DailyExerciseHistory.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IDailyExerciseHistoryService
    {
        ApplicationResponse<DailyExerciseHistory> CreateDailyExerciseHistory(AddDailyExerciseHistoryRequest request);
        ApplicationResponse<DailyExerciseHistory> GetDailyExerciseHistoryById(Guid id);
        ApplicationResponse<IEnumerable<DailyExerciseHistory>> GetAllDailyExerciseHistories();
        ApplicationResponse<bool> UpdateDailyExerciseHistory(UpdateDailyExerciseHistoryRequest request);
        ApplicationResponse<bool> DeleteDailyExerciseHistory(Guid id);
        ApplicationResponse<IEnumerable<DailyExerciseHistory>> FindDailyExerciseHistoriesByFields(Dictionary<string, object> filters);
    }
}

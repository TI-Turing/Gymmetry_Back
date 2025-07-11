using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.DailyExercise.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IDailyExerciseService
    {
        ApplicationResponse<DailyExercise> CreateDailyExercise(AddDailyExerciseRequest request);
        ApplicationResponse<DailyExercise> GetDailyExerciseById(Guid id);
        ApplicationResponse<IEnumerable<DailyExercise>> GetAllDailyExercises();
        ApplicationResponse<bool> UpdateDailyExercise(UpdateDailyExerciseRequest request);
        ApplicationResponse<bool> DeleteDailyExercise(Guid id);
        ApplicationResponse<IEnumerable<DailyExercise>> FindDailyExercisesByFields(Dictionary<string, object> filters);
    }
}

using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IDailyExerciseRepository
    {
        DailyExercise CreateDailyExercise(DailyExercise entity);
        DailyExercise GetDailyExerciseById(Guid id);
        IEnumerable<DailyExercise> GetAllDailyExercises();
        bool UpdateDailyExercise(DailyExercise entity);
        bool DeleteDailyExercise(Guid id);
        IEnumerable<DailyExercise> FindDailyExercisesByFields(Dictionary<string, object> filters);
    }
}

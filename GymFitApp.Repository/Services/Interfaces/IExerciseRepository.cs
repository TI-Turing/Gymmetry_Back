using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IExerciseRepository
    {
        Exercise CreateExercise(Exercise entity);
        Exercise GetExerciseById(Guid id);
        IEnumerable<Exercise> GetAllExercises();
        bool UpdateExercise(Exercise entity);
        bool DeleteExercise(Guid id);
        IEnumerable<Exercise> FindExercisesByFields(Dictionary<string, object> filters);
    }
}

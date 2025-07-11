using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IRoutineExerciseRepository
    {
        RoutineExercise CreateRoutineExercise(RoutineExercise entity);
        RoutineExercise GetRoutineExerciseById(Guid id);
        IEnumerable<RoutineExercise> GetAllRoutineExercises();
        bool UpdateRoutineExercise(RoutineExercise entity);
        bool DeleteRoutineExercise(Guid id);
        IEnumerable<RoutineExercise> FindRoutineExercisesByFields(Dictionary<string, object> filters);
    }
}

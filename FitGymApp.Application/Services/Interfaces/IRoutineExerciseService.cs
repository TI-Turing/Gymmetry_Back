using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.RoutineExercise.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IRoutineExerciseService
    {
        ApplicationResponse<RoutineExercise> CreateRoutineExercise(AddRoutineExerciseRequest request);
        ApplicationResponse<RoutineExercise> GetRoutineExerciseById(Guid id);
        ApplicationResponse<IEnumerable<RoutineExercise>> GetAllRoutineExercises();
        ApplicationResponse<bool> UpdateRoutineExercise(UpdateRoutineExerciseRequest request);
        ApplicationResponse<bool> DeleteRoutineExercise(Guid id);
        ApplicationResponse<IEnumerable<RoutineExercise>> FindRoutineExercisesByFields(Dictionary<string, object> filters);
    }
}

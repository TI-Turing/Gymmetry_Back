using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Exercise.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IExerciseService
    {
        ApplicationResponse<Exercise> CreateExercise(AddExerciseRequest request);
        ApplicationResponse<Exercise> GetExerciseById(Guid id);
        ApplicationResponse<IEnumerable<Exercise>> GetAllExercises();
        ApplicationResponse<bool> UpdateExercise(UpdateExerciseRequest request);
        ApplicationResponse<bool> DeleteExercise(Guid id);
        ApplicationResponse<IEnumerable<Exercise>> FindExercisesByFields(Dictionary<string, object> filters);
    }
}

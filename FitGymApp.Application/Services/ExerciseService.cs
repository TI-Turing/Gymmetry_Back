using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Exercise.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public ExerciseService(IExerciseRepository exerciseRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _exerciseRepository = exerciseRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<Exercise> CreateExercise(AddExerciseRequest request)
        {
            try
            {
                var entity = new Exercise
                {
                    Name = request.Name,
                    Description = request.Description,
                    CategoryExerciseId = request.CategoryExerciseId,
                    Ip = request.Ip
                };
                var created = _exerciseRepository.CreateExercise(entity);
                return new ApplicationResponse<Exercise>
                {
                    Success = true,
                    Message = "Ejercicio creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Exercise>
                {
                    Success = false,
                    Message = "Error técnico al crear el ejercicio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Exercise> GetExerciseById(Guid id)
        {
            var entity = _exerciseRepository.GetExerciseById(id);
            if (entity == null)
            {
                return new ApplicationResponse<Exercise>
                {
                    Success = false,
                    Message = "Ejercicio no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Exercise>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<Exercise>> GetAllExercises()
        {
            var entities = _exerciseRepository.GetAllExercises();
            return new ApplicationResponse<IEnumerable<Exercise>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateExercise(UpdateExerciseRequest request)
        {
            try
            {
                var before = _exerciseRepository.GetExerciseById(request.Id);
                var entity = new Exercise
                {
                    Id = request.Id,
                    Name = request.Name,
                    Description = request.Description,
                    CategoryExerciseId = request.CategoryExerciseId,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _exerciseRepository.UpdateExercise(entity);
                if (updated)
                {
                    _logChangeService.LogChange("Exercise", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Ejercicio actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el ejercicio (no encontrado o inactivo).",
                        ErrorCode = "NotFound"
                    };
                }
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al actualizar el ejercicio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteExercise(Guid id)
        {
            try
            {
                var deleted = _exerciseRepository.DeleteExercise(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Ejercicio eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Ejercicio no encontrado o ya eliminado.",
                        ErrorCode = "NotFound"
                    };
                }
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al eliminar el ejercicio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Exercise>> FindExercisesByFields(Dictionary<string, object> filters)
        {
            var entities = _exerciseRepository.FindExercisesByFields(filters);
            return new ApplicationResponse<IEnumerable<Exercise>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

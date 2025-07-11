using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.DailyExercise.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class DailyExerciseService : IDailyExerciseService
    {
        private readonly IDailyExerciseRepository _dailyExerciseRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public DailyExerciseService(IDailyExerciseRepository dailyExerciseRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _dailyExerciseRepository = dailyExerciseRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<DailyExercise> CreateDailyExercise(AddDailyExerciseRequest request)
        {
            try
            {
                var entity = new DailyExercise
                {
                    UserId = request.UserId,
                    ExerciseId = request.ExerciseId,
                    Date = request.Date,
                    Repetitions = request.Repetitions,
                    Weight = request.Weight,
                    Ip = request.Ip
                };
                var created = _dailyExerciseRepository.CreateDailyExercise(entity);
                return new ApplicationResponse<DailyExercise>
                {
                    Success = true,
                    Message = "Ejercicio diario creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<DailyExercise>
                {
                    Success = false,
                    Message = "Error técnico al crear el ejercicio diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<DailyExercise> GetDailyExerciseById(Guid id)
        {
            var entity = _dailyExerciseRepository.GetDailyExerciseById(id);
            if (entity == null)
            {
                return new ApplicationResponse<DailyExercise>
                {
                    Success = false,
                    Message = "Ejercicio diario no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<DailyExercise>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<DailyExercise>> GetAllDailyExercises()
        {
            var entities = _dailyExerciseRepository.GetAllDailyExercises();
            return new ApplicationResponse<IEnumerable<DailyExercise>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateDailyExercise(UpdateDailyExerciseRequest request)
        {
            try
            {
                var before = _dailyExerciseRepository.GetDailyExerciseById(request.Id);
                var entity = new DailyExercise
                {
                    Id = request.Id,
                    UserId = request.UserId,
                    ExerciseId = request.ExerciseId,
                    Date = request.Date,
                    Repetitions = request.Repetitions,
                    Weight = request.Weight,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _dailyExerciseRepository.UpdateDailyExercise(entity);
                if (updated)
                {
                    _logChangeService.LogChange("DailyExercise", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Ejercicio diario actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el ejercicio diario (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el ejercicio diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteDailyExercise(Guid id)
        {
            try
            {
                var deleted = _dailyExerciseRepository.DeleteDailyExercise(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Ejercicio diario eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Ejercicio diario no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el ejercicio diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<DailyExercise>> FindDailyExercisesByFields(Dictionary<string, object> filters)
        {
            var entities = _dailyExerciseRepository.FindDailyExercisesByFields(filters);
            return new ApplicationResponse<IEnumerable<DailyExercise>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.DailyExerciseHistory.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class DailyExerciseHistoryService : IDailyExerciseHistoryService
    {
        private readonly IDailyExerciseHistoryRepository _dailyExerciseHistoryRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public DailyExerciseHistoryService(IDailyExerciseHistoryRepository dailyExerciseHistoryRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _dailyExerciseHistoryRepository = dailyExerciseHistoryRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<DailyExerciseHistory> CreateDailyExerciseHistory(AddDailyExerciseHistoryRequest request)
        {
            try
            {
                var entity = new DailyExerciseHistory
                {
                    UserId = request.UserId,
                    ExerciseId = request.ExerciseId,
                    Date = request.Date,
                    Repetitions = request.Repetitions,
                    Weight = request.Weight,
                    Ip = request.Ip
                };
                var created = _dailyExerciseHistoryRepository.CreateDailyExerciseHistory(entity);
                return new ApplicationResponse<DailyExerciseHistory>
                {
                    Success = true,
                    Message = "Historial de ejercicio diario creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<DailyExerciseHistory>
                {
                    Success = false,
                    Message = "Error técnico al crear el historial de ejercicio diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<DailyExerciseHistory> GetDailyExerciseHistoryById(Guid id)
        {
            var entity = _dailyExerciseHistoryRepository.GetDailyExerciseHistoryById(id);
            if (entity == null)
            {
                return new ApplicationResponse<DailyExerciseHistory>
                {
                    Success = false,
                    Message = "Historial de ejercicio diario no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<DailyExerciseHistory>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<DailyExerciseHistory>> GetAllDailyExerciseHistories()
        {
            var entities = _dailyExerciseHistoryRepository.GetAllDailyExerciseHistories();
            return new ApplicationResponse<IEnumerable<DailyExerciseHistory>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateDailyExerciseHistory(UpdateDailyExerciseHistoryRequest request)
        {
            try
            {
                var before = _dailyExerciseHistoryRepository.GetDailyExerciseHistoryById(request.Id);
                var entity = new DailyExerciseHistory
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
                var updated = _dailyExerciseHistoryRepository.UpdateDailyExerciseHistory(entity);
                if (updated)
                {
                    _logChangeService.LogChange("DailyExerciseHistory", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Historial de ejercicio diario actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el historial de ejercicio diario (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el historial de ejercicio diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteDailyExerciseHistory(Guid id)
        {
            try
            {
                var deleted = _dailyExerciseHistoryRepository.DeleteDailyExerciseHistory(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Historial de ejercicio diario eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Historial de ejercicio diario no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el historial de ejercicio diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<DailyExerciseHistory>> FindDailyExerciseHistoriesByFields(Dictionary<string, object> filters)
        {
            var entities = _dailyExerciseHistoryRepository.FindDailyExerciseHistoriesByFields(filters);
            return new ApplicationResponse<IEnumerable<DailyExerciseHistory>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.DailyHistory.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class DailyHistoryService : IDailyHistoryService
    {
        private readonly IDailyHistoryRepository _dailyHistoryRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public DailyHistoryService(IDailyHistoryRepository dailyHistoryRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _dailyHistoryRepository = dailyHistoryRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<DailyHistory> CreateDailyHistory(AddDailyHistoryRequest request)
        {
            try
            {
                var entity = new DailyHistory
                {
                    UserId = request.UserId,
                    Date = request.Date,
                    Notes = request.Notes,
                    Ip = request.Ip
                };
                var created = _dailyHistoryRepository.CreateDailyHistory(entity);
                return new ApplicationResponse<DailyHistory>
                {
                    Success = true,
                    Message = "Historial diario creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<DailyHistory>
                {
                    Success = false,
                    Message = "Error técnico al crear el historial diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<DailyHistory> GetDailyHistoryById(Guid id)
        {
            var entity = _dailyHistoryRepository.GetDailyHistoryById(id);
            if (entity == null)
            {
                return new ApplicationResponse<DailyHistory>
                {
                    Success = false,
                    Message = "Historial diario no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<DailyHistory>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<DailyHistory>> GetAllDailyHistories()
        {
            var entities = _dailyHistoryRepository.GetAllDailyHistories();
            return new ApplicationResponse<IEnumerable<DailyHistory>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateDailyHistory(UpdateDailyHistoryRequest request)
        {
            try
            {
                var before = _dailyHistoryRepository.GetDailyHistoryById(request.Id);
                var entity = new DailyHistory
                {
                    Id = request.Id,
                    UserId = request.UserId,
                    Date = request.Date,
                    Notes = request.Notes,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _dailyHistoryRepository.UpdateDailyHistory(entity);
                if (updated)
                {
                    _logChangeService.LogChange("DailyHistory", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Historial diario actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el historial diario (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el historial diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteDailyHistory(Guid id)
        {
            try
            {
                var deleted = _dailyHistoryRepository.DeleteDailyHistory(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Historial diario eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Historial diario no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el historial diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<DailyHistory>> FindDailyHistoriesByFields(Dictionary<string, object> filters)
        {
            var entities = _dailyHistoryRepository.FindDailyHistoriesByFields(filters);
            return new ApplicationResponse<IEnumerable<DailyHistory>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

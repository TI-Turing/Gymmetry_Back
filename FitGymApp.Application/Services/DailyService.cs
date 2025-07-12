using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Daily.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;
using AutoMapper;

namespace FitGymApp.Application.Services
{
    public class DailyService : IDailyService
    {
        private readonly IDailyRepository _dailyRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly IMapper _mapper;

        public DailyService(IDailyRepository dailyRepository, ILogChangeService logChangeService, ILogErrorService logErrorService, IMapper mapper)
        {
            _dailyRepository = dailyRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _mapper = mapper;
        }

        public ApplicationResponse<Daily> CreateDaily(AddDailyRequest request)
        {
            try
            {
                var daily = _mapper.Map<Daily>(request);
                var created = _dailyRepository.CreateDaily(daily);
                return new ApplicationResponse<Daily>
                {
                    Success = true,
                    Message = "Registro diario creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Daily>
                {
                    Success = false,
                    Message = "Error técnico al crear el registro diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Daily> GetDailyById(Guid id)
        {
            var daily = _dailyRepository.GetDailyById(id);
            if (daily == null)
            {
                return new ApplicationResponse<Daily>
                {
                    Success = false,
                    Message = "Registro diario no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Daily>
            {
                Success = true,
                Data = daily
            };
        }

        public ApplicationResponse<IEnumerable<Daily>> GetAllDailies()
        {
            var dailies = _dailyRepository.GetAllDailies();
            return new ApplicationResponse<IEnumerable<Daily>>
            {
                Success = true,
                Data = dailies
            };
        }

        public ApplicationResponse<bool> UpdateDaily(UpdateDailyRequest request)
        {
            try
            {
                var dailyBefore = _dailyRepository.GetDailyById(request.Id);
                var daily = _mapper.Map<Daily>(request);
                var updated = _dailyRepository.UpdateDaily(daily);
                if (updated)
                {
                    _logChangeService.LogChange("Daily", dailyBefore, daily.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Registro diario actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el registro diario (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el registro diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteDaily(Guid id)
        {
            try
            {
                var deleted = _dailyRepository.DeleteDaily(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Registro diario eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Registro diario no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el registro diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Daily>> FindDailiesByFields(Dictionary<string, object> filters)
        {
            var dailies = _dailyRepository.FindDailiesByFields(filters);
            return new ApplicationResponse<IEnumerable<Daily>>
            {
                Success = true,
                Data = dailies
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.Models;
using Gymmetry.Domain.DTO.Daily.Request;
using Gymmetry.Domain.DTO;
using Gymmetry.Repository.Services.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Application.Services
{
    public class DailyService : IDailyService
    {
        private readonly IDailyRepository _dailyRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly IMapper _mapper;
        private readonly ILogger<DailyService> _logger;

        public DailyService(IDailyRepository dailyRepository, ILogChangeService logChangeService, ILogErrorService logErrorService, IMapper mapper, ILogger<DailyService> logger)
        {
            _dailyRepository = dailyRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApplicationResponse<Daily>> CreateDailyAsync(AddDailyRequest request)
        {
            _logger.LogInformation("[DailyService] Inicio CreateDailyAsync");
            try
            {
                if (request == null)
                    return ApplicationResponse<Daily>.ErrorResponse("Request nulo", "BadRequest");
                var entity = _mapper.Map<Daily>(request);
                entity.CreatedAt = DateTime.UtcNow;
                entity.IsActive = true;
                var created = await _dailyRepository.CreateDailyAsync(entity);
                _logger.LogInformation($"[DailyService] Daily creado: {created.Id}");
                return ApplicationResponse<Daily>.SuccessResponse(created, "Daily creado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DailyService] Error en CreateDailyAsync");
                return ApplicationResponse<Daily>.ErrorResponse("Error técnico al crear Daily", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<Daily>> GetDailyByIdAsync(Guid id)
        {
            var daily = await _dailyRepository.GetDailyByIdAsync(id);
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

        public async Task<ApplicationResponse<IEnumerable<Daily>>> GetAllDailiesAsync()
        {
            var dailies = await _dailyRepository.GetAllDailiesAsync();
            return new ApplicationResponse<IEnumerable<Daily>>
            {
                Success = true,
                Data = dailies
            };
        }

        public async Task<ApplicationResponse<bool>> UpdateDailyAsync(UpdateDailyRequest request, Guid? userId, string ip = "", string invocationId = "")
        {
            try
            {
                var dailyBefore = await _dailyRepository.GetDailyByIdAsync(request.Id);
                var daily = _mapper.Map<Daily>(request);
                var updated = await _dailyRepository.UpdateDailyAsync(daily);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("Daily", dailyBefore, userId, ip, invocationId);
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
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al actualizar el registro diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteDailyAsync(Guid id)
        {
            try
            {
                var deleted = await _dailyRepository.DeleteDailyAsync(id);
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
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al eliminar el registro diario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<Daily>>> FindDailiesByFieldsAsync(Dictionary<string, object> filters)
        {
            var dailies = await _dailyRepository.FindDailiesByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<Daily>>
            {
                Success = true,
                Data = dailies
            };
        }
    }
}

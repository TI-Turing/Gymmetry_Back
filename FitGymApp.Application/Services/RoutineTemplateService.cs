using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.RoutineTemplate.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class RoutineTemplateService : IRoutineTemplateService
    {
        private readonly IRoutineTemplateRepository _routineTemplateRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public RoutineTemplateService(IRoutineTemplateRepository routineTemplateRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _routineTemplateRepository = routineTemplateRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public async Task<ApplicationResponse<RoutineTemplate>> CreateRoutineTemplateAsync(AddRoutineTemplateRequest request)
        {
            try
            {
                var entity = new RoutineTemplate
                {
                    Name = request.Name,
                    Comments = request.Comments,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    GymId = request.GymId,
                    RoutineUserRoutineId = request.RoutineUserRoutineId,
                    RoutineAssignedId = request.RoutineAssignedId
                };
                var created = await _routineTemplateRepository.CreateRoutineTemplateAsync(entity);
                return new ApplicationResponse<RoutineTemplate>
                {
                    Success = true,
                    Message = "Plantilla de rutina creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<RoutineTemplate>
                {
                    Success = false,
                    Message = "Error técnico al crear la plantilla de rutina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<RoutineTemplate>> GetRoutineTemplateByIdAsync(Guid id)
        {
            var entity = await _routineTemplateRepository.GetRoutineTemplateByIdAsync(id);
            if (entity == null)
            {
                return new ApplicationResponse<RoutineTemplate>
                {
                    Success = false,
                    Message = "Plantilla de rutina no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<RoutineTemplate>
            {
                Success = true,
                Data = entity
            };
        }

        public async Task<ApplicationResponse<IEnumerable<RoutineTemplate>>> GetAllRoutineTemplatesAsync()
        {
            var entities = await _routineTemplateRepository.GetAllRoutineTemplatesAsync();
            return new ApplicationResponse<IEnumerable<RoutineTemplate>>
            {
                Success = true,
                Data = entities
            };
        }

        public async Task<ApplicationResponse<bool>> UpdateRoutineTemplateAsync(UpdateRoutineTemplateRequest request)
        {
            try
            {
                var before = await _routineTemplateRepository.GetRoutineTemplateByIdAsync(request.Id);
                var entity = new RoutineTemplate
                {
                    Id = request.Id,
                    Name = request.Name,
                    Comments = request.Comments,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    GymId = request.GymId,
                    RoutineUserRoutineId = request.RoutineUserRoutineId,
                    RoutineAssignedId = request.RoutineAssignedId
                };
                var updated = await _routineTemplateRepository.UpdateRoutineTemplateAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("RoutineTemplate", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Plantilla de rutina actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la plantilla de rutina (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la plantilla de rutina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteRoutineTemplateAsync(Guid id)
        {
            try
            {
                var deleted = await _routineTemplateRepository.DeleteRoutineTemplateAsync(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Plantilla de rutina eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Plantilla de rutina no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la plantilla de rutina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<RoutineTemplate>>> FindRoutineTemplatesByFieldsAsync(Dictionary<string, object> filters)
        {
            var entities = await _routineTemplateRepository.FindRoutineTemplatesByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<RoutineTemplate>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

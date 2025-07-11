using System;
using System.Collections.Generic;
using System.Linq;
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

        public ApplicationResponse<RoutineTemplate> CreateRoutineTemplate(AddRoutineTemplateRequest request)
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
                var created = _routineTemplateRepository.CreateRoutineTemplate(entity);
                return new ApplicationResponse<RoutineTemplate>
                {
                    Success = true,
                    Message = "Plantilla de rutina creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<RoutineTemplate>
                {
                    Success = false,
                    Message = "Error técnico al crear la plantilla de rutina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<RoutineTemplate> GetRoutineTemplateById(Guid id)
        {
            var entity = _routineTemplateRepository.GetRoutineTemplateById(id);
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

        public ApplicationResponse<IEnumerable<RoutineTemplate>> GetAllRoutineTemplates()
        {
            var entities = _routineTemplateRepository.GetAllRoutineTemplates();
            return new ApplicationResponse<IEnumerable<RoutineTemplate>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateRoutineTemplate(UpdateRoutineTemplateRequest request)
        {
            try
            {
                var before = _routineTemplateRepository.GetRoutineTemplateById(request.Id);
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
                var updated = _routineTemplateRepository.UpdateRoutineTemplate(entity);
                if (updated)
                {
                    _logChangeService.LogChange("RoutineTemplate", before, entity.Id);
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
                _logErrorService.LogError(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al actualizar la plantilla de rutina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteRoutineTemplate(Guid id)
        {
            try
            {
                var deleted = _routineTemplateRepository.DeleteRoutineTemplate(id);
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
                _logErrorService.LogError(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al eliminar la plantilla de rutina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<RoutineTemplate>> FindRoutineTemplatesByFields(Dictionary<string, object> filters)
        {
            var entities = _routineTemplateRepository.FindRoutineTemplatesByFields(filters);
            return new ApplicationResponse<IEnumerable<RoutineTemplate>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

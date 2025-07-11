using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.RoutineAssigned.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class RoutineAssignedService : IRoutineAssignedService
    {
        private readonly IRoutineAssignedRepository _routineAssignedRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public RoutineAssignedService(IRoutineAssignedRepository routineAssignedRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _routineAssignedRepository = routineAssignedRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<RoutineAssigned> CreateRoutineAssigned(AddRoutineAssignedRequest request)
        {
            try
            {
                var entity = new RoutineAssigned
                {
                    Comments = request.Comments,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    UserId = request.UserId
                };
                var created = _routineAssignedRepository.CreateRoutineAssigned(entity);
                return new ApplicationResponse<RoutineAssigned>
                {
                    Success = true,
                    Message = "Rutina asignada creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<RoutineAssigned>
                {
                    Success = false,
                    Message = "Error técnico al crear la rutina asignada.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<RoutineAssigned> GetRoutineAssignedById(Guid id)
        {
            var entity = _routineAssignedRepository.GetRoutineAssignedById(id);
            if (entity == null)
            {
                return new ApplicationResponse<RoutineAssigned>
                {
                    Success = false,
                    Message = "Rutina asignada no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<RoutineAssigned>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<RoutineAssigned>> GetAllRoutineAssigneds()
        {
            var entities = _routineAssignedRepository.GetAllRoutineAssigneds();
            return new ApplicationResponse<IEnumerable<RoutineAssigned>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateRoutineAssigned(UpdateRoutineAssignedRequest request)
        {
            try
            {
                var before = _routineAssignedRepository.GetRoutineAssignedById(request.Id);
                var entity = new RoutineAssigned
                {
                    Id = request.Id,
                    Comments = request.Comments,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    UserId = request.UserId
                };
                var updated = _routineAssignedRepository.UpdateRoutineAssigned(entity);
                if (updated)
                {
                    _logChangeService.LogChange("RoutineAssigned", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Rutina asignada actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la rutina asignada (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la rutina asignada.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteRoutineAssigned(Guid id)
        {
            try
            {
                var deleted = _routineAssignedRepository.DeleteRoutineAssigned(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Rutina asignada eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Rutina asignada no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la rutina asignada.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<RoutineAssigned>> FindRoutineAssignedsByFields(Dictionary<string, object> filters)
        {
            var entities = _routineAssignedRepository.FindRoutineAssignedsByFields(filters);
            return new ApplicationResponse<IEnumerable<RoutineAssigned>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

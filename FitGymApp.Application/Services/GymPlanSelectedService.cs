using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.GymPlanSelected.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class GymPlanSelectedService : IGymPlanSelectedService
    {
        private readonly IGymPlanSelectedRepository _gymPlanSelectedRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public GymPlanSelectedService(IGymPlanSelectedRepository gymPlanSelectedRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _gymPlanSelectedRepository = gymPlanSelectedRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<GymPlanSelected> CreateGymPlanSelected(AddGymPlanSelectedRequest request)
        {
            try
            {
                var entity = new GymPlanSelected
                {
                    PlanId = request.PlanId,
                    UserId = request.UserId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Ip = request.Ip
                };
                var created = _gymPlanSelectedRepository.CreateGymPlanSelected(entity);
                return new ApplicationResponse<GymPlanSelected>
                {
                    Success = true,
                    Message = "Plan seleccionado de gimnasio creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<GymPlanSelected>
                {
                    Success = false,
                    Message = "Error técnico al crear el plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<GymPlanSelected> GetGymPlanSelectedById(Guid id)
        {
            var entity = _gymPlanSelectedRepository.GetGymPlanSelectedById(id);
            if (entity == null)
            {
                return new ApplicationResponse<GymPlanSelected>
                {
                    Success = false,
                    Message = "Plan seleccionado de gimnasio no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<GymPlanSelected>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<GymPlanSelected>> GetAllGymPlanSelecteds()
        {
            var entities = _gymPlanSelectedRepository.GetAllGymPlanSelecteds();
            return new ApplicationResponse<IEnumerable<GymPlanSelected>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateGymPlanSelected(UpdateGymPlanSelectedRequest request)
        {
            try
            {
                var before = _gymPlanSelectedRepository.GetGymPlanSelectedById(request.Id);
                var entity = new GymPlanSelected
                {
                    Id = request.Id,
                    PlanId = request.PlanId,
                    UserId = request.UserId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _gymPlanSelectedRepository.UpdateGymPlanSelected(entity);
                if (updated)
                {
                    _logChangeService.LogChange("GymPlanSelected", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Plan seleccionado de gimnasio actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el plan seleccionado de gimnasio (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteGymPlanSelected(Guid id)
        {
            try
            {
                var deleted = _gymPlanSelectedRepository.DeleteGymPlanSelected(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Plan seleccionado de gimnasio eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Plan seleccionado de gimnasio no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<GymPlanSelected>> FindGymPlanSelectedsByFields(Dictionary<string, object> filters)
        {
            var entities = _gymPlanSelectedRepository.FindGymPlanSelectedsByFields(filters);
            return new ApplicationResponse<IEnumerable<GymPlanSelected>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

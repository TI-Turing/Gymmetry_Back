using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.PlanType.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class PlanTypeService : IPlanTypeService
    {
        private readonly IPlanTypeRepository _planTypeRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public PlanTypeService(IPlanTypeRepository planTypeRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _planTypeRepository = planTypeRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<PlanType> CreatePlanType(AddPlanTypeRequest request)
        {
            try
            {
                var entity = new PlanType
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _planTypeRepository.CreatePlanType(entity);
                return new ApplicationResponse<PlanType>
                {
                    Success = true,
                    Message = "Tipo de plan creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<PlanType>
                {
                    Success = false,
                    Message = "Error técnico al crear el tipo de plan.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<PlanType> GetPlanTypeById(Guid id)
        {
            var entity = _planTypeRepository.GetPlanTypeById(id);
            if (entity == null)
            {
                return new ApplicationResponse<PlanType>
                {
                    Success = false,
                    Message = "Tipo de plan no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<PlanType>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<PlanType>> GetAllPlanTypes()
        {
            var entities = _planTypeRepository.GetAllPlanTypes();
            return new ApplicationResponse<IEnumerable<PlanType>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdatePlanType(UpdatePlanTypeRequest request)
        {
            try
            {
                var before = _planTypeRepository.GetPlanTypeById(request.Id);
                var entity = new PlanType
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _planTypeRepository.UpdatePlanType(entity);
                if (updated)
                {
                    _logChangeService.LogChange("PlanType", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de plan actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el tipo de plan (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el tipo de plan.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeletePlanType(Guid id)
        {
            try
            {
                var deleted = _planTypeRepository.DeletePlanType(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de plan eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Tipo de plan no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el tipo de plan.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<PlanType>> FindPlanTypesByFields(Dictionary<string, object> filters)
        {
            var entities = _planTypeRepository.FindPlanTypesByFields(filters);
            return new ApplicationResponse<IEnumerable<PlanType>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

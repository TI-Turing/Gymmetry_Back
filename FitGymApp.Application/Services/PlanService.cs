using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Plan.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;
using AutoMapper;

namespace FitGymApp.Application.Services
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly IMapper _mapper;

        public PlanService(IPlanRepository planRepository, ILogChangeService logChangeService, ILogErrorService logErrorService, IMapper mapper)
        {
            _planRepository = planRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _mapper = mapper;
        }

        public ApplicationResponse<Plan> CreatePlan(AddPlanRequest request)
        {
            try
            {
                var entity = _mapper.Map<Plan>(request);
                var created = _planRepository.CreatePlan(entity);
                return new ApplicationResponse<Plan>
                {
                    Success = true,
                    Message = "Plan creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Plan>
                {
                    Success = false,
                    Message = "Error técnico al crear el plan.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Plan> GetPlanById(Guid id)
        {
            var entity = _planRepository.GetPlanById(id);
            if (entity == null)
            {
                return new ApplicationResponse<Plan>
                {
                    Success = false,
                    Message = "Plan no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Plan>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<Plan>> GetAllPlans()
        {
            var entities = _planRepository.GetAllPlans();
            return new ApplicationResponse<IEnumerable<Plan>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdatePlan(UpdatePlanRequest request)
        {
            try
            {
                var before = _planRepository.GetPlanById(request.Id);
                var entity = _mapper.Map<Plan>(request);
                var updated = _planRepository.UpdatePlan(entity);
                if (updated)
                {
                    _logChangeService.LogChange("Plan", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Plan actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el plan (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el plan.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeletePlan(Guid id)
        {
            try
            {
                var deleted = _planRepository.DeletePlan(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Plan eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Plan no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el plan.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Plan>> FindPlansByFields(Dictionary<string, object> filters)
        {
            var entities = _planRepository.FindPlansByFields(filters);
            return new ApplicationResponse<IEnumerable<Plan>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

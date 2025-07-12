using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.GymPlanSelectedModule.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;
using AutoMapper;

namespace FitGymApp.Application.Services
{
    public class GymPlanSelectedModuleService : IGymPlanSelectedModuleService
    {
        private readonly IGymPlanSelectedModuleRepository _gymPlanSelectedModuleRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly IMapper _mapper;

        public GymPlanSelectedModuleService(IGymPlanSelectedModuleRepository gymPlanSelectedModuleRepository, ILogChangeService logChangeService, ILogErrorService logErrorService, IMapper mapper)
        {
            _gymPlanSelectedModuleRepository = gymPlanSelectedModuleRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _mapper = mapper;
        }

        public ApplicationResponse<GymPlanSelectedModule> CreateGymPlanSelectedModule(AddGymPlanSelectedModuleRequest request)
        {
            try
            {
                var entity = _mapper.Map<GymPlanSelectedModule>(request);
                var created = _gymPlanSelectedModuleRepository.CreateGymPlanSelectedModule(entity);
                return new ApplicationResponse<GymPlanSelectedModule>
                {
                    Success = true,
                    Message = "Módulo de plan seleccionado de gimnasio creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<GymPlanSelectedModule>
                {
                    Success = false,
                    Message = "Error técnico al crear el módulo de plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<GymPlanSelectedModule> GetGymPlanSelectedModuleById(Guid id)
        {
            var entity = _gymPlanSelectedModuleRepository.GetGymPlanSelectedModuleById(id);
            if (entity == null)
            {
                return new ApplicationResponse<GymPlanSelectedModule>
                {
                    Success = false,
                    Message = "Módulo de plan seleccionado de gimnasio no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<GymPlanSelectedModule>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<GymPlanSelectedModule>> GetAllGymPlanSelectedModules()
        {
            var entities = _gymPlanSelectedModuleRepository.GetAllGymPlanSelectedModules();
            return new ApplicationResponse<IEnumerable<GymPlanSelectedModule>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateGymPlanSelectedModule(UpdateGymPlanSelectedModuleRequest request)
        {
            try
            {
                var before = _gymPlanSelectedModuleRepository.GetGymPlanSelectedModuleById(request.Id);
                var entity = _mapper.Map<GymPlanSelectedModule>(request);
                var updated = _gymPlanSelectedModuleRepository.UpdateGymPlanSelectedModule(entity);
                if (updated)
                {
                    _logChangeService.LogChange("GymPlanSelectedModule", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Módulo de plan seleccionado de gimnasio actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el módulo de plan seleccionado de gimnasio (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el módulo de plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteGymPlanSelectedModule(Guid id)
        {
            try
            {
                var deleted = _gymPlanSelectedModuleRepository.DeleteGymPlanSelectedModule(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Módulo de plan seleccionado de gimnasio eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Módulo de plan seleccionado de gimnasio no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el módulo de plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<GymPlanSelectedModule>> FindGymPlanSelectedModulesByFields(Dictionary<string, object> filters)
        {
            var entities = _gymPlanSelectedModuleRepository.FindGymPlanSelectedModulesByFields(filters);
            return new ApplicationResponse<IEnumerable<GymPlanSelectedModule>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

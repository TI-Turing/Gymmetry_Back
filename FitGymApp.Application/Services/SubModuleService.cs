using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.SubModule.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class SubModuleService : ISubModuleService
    {
        private readonly ISubModuleRepository _subModuleRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public SubModuleService(ISubModuleRepository subModuleRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _subModuleRepository = subModuleRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<SubModule> CreateSubModule(AddSubModuleRequest request)
        {
            try
            {
                var entity = new SubModule
                {
                    BranchId = request.BranchId,
                    Ip = request.Ip
                };
                var created = _subModuleRepository.CreateSubModule(entity);
                return new ApplicationResponse<SubModule>
                {
                    Success = true,
                    Message = "Submódulo creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<SubModule>
                {
                    Success = false,
                    Message = "Error técnico al crear el submódulo.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<SubModule> GetSubModuleById(Guid id)
        {
            var entity = _subModuleRepository.GetSubModuleById(id);
            if (entity == null)
            {
                return new ApplicationResponse<SubModule>
                {
                    Success = false,
                    Message = "Submódulo no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<SubModule>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<SubModule>> GetAllSubModules()
        {
            var entities = _subModuleRepository.GetAllSubModules();
            return new ApplicationResponse<IEnumerable<SubModule>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateSubModule(UpdateSubModuleRequest request)
        {
            try
            {
                var before = _subModuleRepository.GetSubModuleById(request.Id);
                var entity = new SubModule
                {
                    Id = request.Id,
                    BranchId = request.BranchId,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _subModuleRepository.UpdateSubModule(entity);
                if (updated)
                {
                    _logChangeService.LogChange("SubModule", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Submódulo actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el submódulo (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el submódulo.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteSubModule(Guid id)
        {
            try
            {
                var deleted = _subModuleRepository.DeleteSubModule(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Submódulo eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Submódulo no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el submódulo.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<SubModule>> FindSubModulesByFields(Dictionary<string, object> filters)
        {
            var entities = _subModuleRepository.FindSubModulesByFields(filters);
            return new ApplicationResponse<IEnumerable<SubModule>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

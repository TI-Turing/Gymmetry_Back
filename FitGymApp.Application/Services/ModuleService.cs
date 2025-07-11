using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Module.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public ModuleService(IModuleRepository moduleRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _moduleRepository = moduleRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<Module> CreateModule(AddModuleRequest request)
        {
            try
            {
                var entity = new Module
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _moduleRepository.CreateModule(entity);
                return new ApplicationResponse<Module>
                {
                    Success = true,
                    Message = "Módulo creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Module>
                {
                    Success = false,
                    Message = "Error técnico al crear el módulo.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Module> GetModuleById(Guid id)
        {
            var entity = _moduleRepository.GetModuleById(id);
            if (entity == null)
            {
                return new ApplicationResponse<Module>
                {
                    Success = false,
                    Message = "Módulo no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Module>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<Module>> GetAllModules()
        {
            var entities = _moduleRepository.GetAllModules();
            return new ApplicationResponse<IEnumerable<Module>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateModule(UpdateModuleRequest request)
        {
            try
            {
                var before = _moduleRepository.GetModuleById(request.Id);
                var entity = new Module
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _moduleRepository.UpdateModule(entity);
                if (updated)
                {
                    _logChangeService.LogChange("Module", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Módulo actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el módulo (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el módulo.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteModule(Guid id)
        {
            try
            {
                var deleted = _moduleRepository.DeleteModule(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Módulo eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Módulo no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el módulo.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Module>> FindModulesByFields(Dictionary<string, object> filters)
        {
            var entities = _moduleRepository.FindModulesByFields(filters);
            return new ApplicationResponse<IEnumerable<Module>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

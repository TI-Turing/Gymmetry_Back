using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.AccessMethodType.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class AccessMethodTypeService : IAccessMethodTypeService
    {
        private readonly IAccessMethodTypeRepository _accessMethodTypeRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public AccessMethodTypeService(IAccessMethodTypeRepository accessMethodTypeRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _accessMethodTypeRepository = accessMethodTypeRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<AccessMethodType> CreateAccessMethodType(AddAccessMethodTypeRequest request)
        {
            try
            {
                var entity = new AccessMethodType
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _accessMethodTypeRepository.CreateAccessMethodType(entity);
                return new ApplicationResponse<AccessMethodType>
                {
                    Success = true,
                    Message = "Método de acceso creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<AccessMethodType>
                {
                    Success = false,
                    Message = "Error técnico al crear el método de acceso.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<AccessMethodType> GetAccessMethodTypeById(Guid id)
        {
            var entity = _accessMethodTypeRepository.GetAccessMethodTypeById(id);
            if (entity == null)
            {
                return new ApplicationResponse<AccessMethodType>
                {
                    Success = false,
                    Message = "Método de acceso no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<AccessMethodType>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<AccessMethodType>> GetAllAccessMethodTypes()
        {
            var entities = _accessMethodTypeRepository.GetAllAccessMethodTypes();
            return new ApplicationResponse<IEnumerable<AccessMethodType>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateAccessMethodType(UpdateAccessMethodTypeRequest request)
        {
            try
            {
                var before = _accessMethodTypeRepository.GetAccessMethodTypeById(request.Id);
                var entity = new AccessMethodType
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _accessMethodTypeRepository.UpdateAccessMethodType(entity);
                if (updated)
                {
                    _logChangeService.LogChange("AccessMethodType", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Método de acceso actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el método de acceso (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el método de acceso.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteAccessMethodType(Guid id)
        {
            try
            {
                var deleted = _accessMethodTypeRepository.DeleteAccessMethodType(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Método de acceso eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Método de acceso no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el método de acceso.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<AccessMethodType>> FindAccessMethodTypesByFields(Dictionary<string, object> filters)
        {
            var entities = _accessMethodTypeRepository.FindAccessMethodTypesByFields(filters);
            return new ApplicationResponse<IEnumerable<AccessMethodType>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

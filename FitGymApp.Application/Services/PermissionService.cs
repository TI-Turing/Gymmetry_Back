using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Permission.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public PermissionService(IPermissionRepository permissionRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _permissionRepository = permissionRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<Permission> CreatePermission(AddPermissionRequest request)
        {
            try
            {
                var entity = new Permission
                {
                    See = request.See,
                    Create = request.Create,
                    Read = request.Read,
                    Update = request.Update,
                    Delete = request.Delete,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    UserTypeId = request.UserTypeId,
                    UserId = request.UserId
                };
                var created = _permissionRepository.CreatePermission(entity);
                return new ApplicationResponse<Permission>
                {
                    Success = true,
                    Message = "Permiso creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Permission>
                {
                    Success = false,
                    Message = "Error técnico al crear el permiso.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Permission> GetPermissionById(Guid id)
        {
            var entity = _permissionRepository.GetPermissionById(id);
            if (entity == null)
            {
                return new ApplicationResponse<Permission>
                {
                    Success = false,
                    Message = "Permiso no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Permission>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<Permission>> GetAllPermissions()
        {
            var entities = _permissionRepository.GetAllPermissions();
            return new ApplicationResponse<IEnumerable<Permission>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdatePermission(UpdatePermissionRequest request)
        {
            try
            {
                var before = _permissionRepository.GetPermissionById(request.Id);
                var entity = new Permission
                {
                    Id = request.Id,
                    See = request.See,
                    Create = request.Create,
                    Read = request.Read,
                    Update = request.Update,
                    Delete = request.Delete,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    UserTypeId = request.UserTypeId,
                    UserId = request.UserId
                };
                var updated = _permissionRepository.UpdatePermission(entity);
                if (updated)
                {
                    _logChangeService.LogChange("Permission", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Permiso actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el permiso (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el permiso.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeletePermission(Guid id)
        {
            try
            {
                var deleted = _permissionRepository.DeletePermission(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Permiso eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Permiso no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el permiso.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Permission>> FindPermissionsByFields(Dictionary<string, object> filters)
        {
            var entities = _permissionRepository.FindPermissionsByFields(filters);
            return new ApplicationResponse<IEnumerable<Permission>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

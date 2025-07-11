using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.UserType.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class UserTypeService : IUserTypeService
    {
        private readonly IUserTypeRepository _userTypeRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public UserTypeService(IUserTypeRepository userTypeRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _userTypeRepository = userTypeRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<UserType> CreateUserType(AddUserTypeRequest request)
        {
            try
            {
                var entity = new UserType
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _userTypeRepository.CreateUserType(entity);
                return new ApplicationResponse<UserType>
                {
                    Success = true,
                    Message = "Tipo de usuario creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<UserType>
                {
                    Success = false,
                    Message = "Error técnico al crear el tipo de usuario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<UserType> GetUserTypeById(Guid id)
        {
            var entity = _userTypeRepository.GetUserTypeById(id);
            if (entity == null)
            {
                return new ApplicationResponse<UserType>
                {
                    Success = false,
                    Message = "Tipo de usuario no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<UserType>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<UserType>> GetAllUserTypes()
        {
            var entities = _userTypeRepository.GetAllUserTypes();
            return new ApplicationResponse<IEnumerable<UserType>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateUserType(UpdateUserTypeRequest request)
        {
            try
            {
                var before = _userTypeRepository.GetUserTypeById(request.Id);
                var entity = new UserType
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _userTypeRepository.UpdateUserType(entity);
                if (updated)
                {
                    _logChangeService.LogChange("UserType", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de usuario actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el tipo de usuario (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el tipo de usuario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteUserType(Guid id)
        {
            try
            {
                var deleted = _userTypeRepository.DeleteUserType(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de usuario eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Tipo de usuario no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el tipo de usuario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<UserType>> FindUserTypesByFields(Dictionary<string, object> filters)
        {
            var entities = _userTypeRepository.FindUserTypesByFields(filters);
            return new ApplicationResponse<IEnumerable<UserType>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

using System;
using System.Collections.Generic;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.User.Request;
using FitGymApp.Domain.DTO;
using System.Linq;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public UserService(IUserRepository userRepository, IPasswordService passwordService, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<User> CreateUser(AddRequest request)
        {
            var existingUsers = _userRepository.FindUsersByFields(new Dictionary<string, object> { { "Email", request.Email } });
            if (existingUsers != null && existingUsers.Any())
            {
                return new ApplicationResponse<User>
                {
                    Success = false,
                    Message = "El correo ya está registrado.",
                    ErrorCode = "EmailExists"
                };
            }
            var hashResult = _passwordService.HashPassword(request.Password);
            if (!hashResult.Success)
            {
                return new ApplicationResponse<User>
                {
                    Success = false,
                    Message = hashResult.Message ?? "Error técnico al hashear el password.",
                    ErrorCode = hashResult.ErrorCode ?? "HashError"
                };
            }
            try
            {
                var user = new User
                {
                    Email = request.Email,
                    Password = hashResult.Data,
                };
                var created = _userRepository.CreateUser(user);
                return new ApplicationResponse<User>
                {
                    Success = true,
                    Message = "Usuario creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<User>
                {
                    Success = false,
                    Message = "Error técnico al crear el usuario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<User> GetUserById(Guid id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null)
            {
                return new ApplicationResponse<User>
                {
                    Success = false,
                    Message = "Usuario no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<User>
            {
                Success = true,
                Data = user
            };
        }

        public ApplicationResponse<IEnumerable<User>> GetAllUsers()
        {
            var users = _userRepository.GetAllUsers();
            return new ApplicationResponse<IEnumerable<User>>
            {
                Success = true,
                Data = users
            };
        }

        public ApplicationResponse<bool> UpdateUser(UpdateRequest request)
        {
            try
            {
                var userBefore = _userRepository.GetUserById(request.Id);
                var user = new User
                {
                    Id = request.Id,
                    IdEps = request.IdEps,
                    Name = request.Name,
                    LastName = request.LastName,
                    UserName = request.UserName,
                    IdGender = request.IdGender,
                    BirthDate = request.BirthDate,
                    DocumentTypeId = request.DocumentTypeId,
                    DocumentType = request.DocumentType,
                    Phone = request.Phone,
                    CountryId = request.CountryId,
                    Address = request.Address,
                    CityId = request.CityId,
                    RegionId = request.RegionId,
                    Rh = request.Rh,
                    EmergencyName = request.EmergencyName,
                    EmergencyPhone = request.EmergencyPhone,
                    PhysicalExceptions = request.PhysicalExceptions,
                    IsActive = request.IsActive,
                    UserTypeId = request.UserTypeId
                };
                var updated = _userRepository.UpdateUser(user);
                if (updated)
                {
                    _logChangeService.LogChange("User", userBefore, user.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Usuario actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el usuario (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el usuario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteUser(Guid id)
        {
            try
            {
                var deleted = _userRepository.DeleteUser(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Usuario eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Usuario no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el usuario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<User>> FindUsersByFields(Dictionary<string, object> filters)
        {
            var users = _userRepository.FindUsersByFields(filters);
            return new ApplicationResponse<IEnumerable<User>>
            {
                Success = true,
                Data = users
            };
        }
    }
}

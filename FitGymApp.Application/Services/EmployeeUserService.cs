using AutoMapper;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO;
using FitGymApp.Domain.DTO.EmployeeUser.Request;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitGymApp.Application.Services
{
    public class EmployeeUserService : IEmployeeUserService
    {
        private readonly IEmployeeUserRepository _employeeUserRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly IMapper _mapper;

        public EmployeeUserService(IEmployeeUserRepository employeeUserRepository, ILogChangeService logChangeService, ILogErrorService logErrorService, IMapper mapper)
        {
            _employeeUserRepository = employeeUserRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _mapper = mapper;
        }

        public ApplicationResponse<EmployeeUser> CreateEmployeeUser(AddEmployeeUserRequest request)
        {
            try
            {
                var entity = _mapper.Map<EmployeeUser>(request);
                var created = _employeeUserRepository.CreateEmployeeUser(entity);
                return new ApplicationResponse<EmployeeUser>
                {
                    Success = true,
                    Message = "Empleado usuario creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<EmployeeUser>
                {
                    Success = false,
                    Message = "Error técnico al crear el empleado usuario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<EmployeeUser> GetEmployeeUserById(Guid id)
        {
            var entity = _employeeUserRepository.GetEmployeeUserById(id);
            if (entity == null)
            {
                return new ApplicationResponse<EmployeeUser>
                {
                    Success = false,
                    Message = "Empleado usuario no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<EmployeeUser>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<EmployeeUser>> GetAllEmployeeUsers()
        {
            var entities = _employeeUserRepository.GetAllEmployeeUsers();
            return new ApplicationResponse<IEnumerable<EmployeeUser>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateEmployeeUser(UpdateEmployeeUserRequest request)
        {
            try
            {
                var before = _employeeUserRepository.GetEmployeeUserById(request.Id);
                var entity = _mapper.Map<EmployeeUser>(request);
                var updated = _employeeUserRepository.UpdateEmployeeUser(entity);
                if (updated)
                {
                    _logChangeService.LogChange("EmployeeUser", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Empleado usuario actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el empleado usuario (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el empleado usuario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteEmployeeUser(Guid id)
        {
            try
            {
                var deleted = _employeeUserRepository.DeleteEmployeeUser(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Empleado usuario eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Empleado usuario no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el empleado usuario.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<EmployeeUser>> FindEmployeeUsersByFields(Dictionary<string, object> filters)
        {
            var entities = _employeeUserRepository.FindEmployeeUsersByFields(filters);
            return new ApplicationResponse<IEnumerable<EmployeeUser>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.EmployeeType.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class EmployeeTypeService : IEmployeeTypeService
    {
        private readonly IEmployeeTypeRepository _employeeTypeRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public EmployeeTypeService(IEmployeeTypeRepository employeeTypeRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _employeeTypeRepository = employeeTypeRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<EmployeeType> CreateEmployeeType(AddEmployeeTypeRequest request)
        {
            try
            {
                var entity = new EmployeeType
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _employeeTypeRepository.CreateEmployeeType(entity);
                return new ApplicationResponse<EmployeeType>
                {
                    Success = true,
                    Message = "Tipo de empleado creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<EmployeeType>
                {
                    Success = false,
                    Message = "Error técnico al crear el tipo de empleado.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<EmployeeType> GetEmployeeTypeById(Guid id)
        {
            var entity = _employeeTypeRepository.GetEmployeeTypeById(id);
            if (entity == null)
            {
                return new ApplicationResponse<EmployeeType>
                {
                    Success = false,
                    Message = "Tipo de empleado no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<EmployeeType>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<EmployeeType>> GetAllEmployeeTypes()
        {
            var entities = _employeeTypeRepository.GetAllEmployeeTypes();
            return new ApplicationResponse<IEnumerable<EmployeeType>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateEmployeeType(UpdateEmployeeTypeRequest request)
        {
            try
            {
                var before = _employeeTypeRepository.GetEmployeeTypeById(request.Id);
                var entity = new EmployeeType
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _employeeTypeRepository.UpdateEmployeeType(entity);
                if (updated)
                {
                    _logChangeService.LogChange("EmployeeType", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de empleado actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el tipo de empleado (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el tipo de empleado.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteEmployeeType(Guid id)
        {
            try
            {
                var deleted = _employeeTypeRepository.DeleteEmployeeType(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de empleado eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Tipo de empleado no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el tipo de empleado.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<EmployeeType>> FindEmployeeTypesByFields(Dictionary<string, object> filters)
        {
            var entities = _employeeTypeRepository.FindEmployeeTypesByFields(filters);
            return new ApplicationResponse<IEnumerable<EmployeeType>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

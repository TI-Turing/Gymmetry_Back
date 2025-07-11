using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.EmployeeRegisterDaily.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class EmployeeRegisterDailyService : IEmployeeRegisterDailyService
    {
        private readonly IEmployeeRegisterDailyRepository _employeeRegisterDailyRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public EmployeeRegisterDailyService(IEmployeeRegisterDailyRepository employeeRegisterDailyRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _employeeRegisterDailyRepository = employeeRegisterDailyRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<EmployeeRegisterDaily> CreateEmployeeRegisterDaily(AddEmployeeRegisterDailyRequest request)
        {
            try
            {
                var entity = new EmployeeRegisterDaily
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Ip = request.Ip
                };
                var created = _employeeRegisterDailyRepository.CreateEmployeeRegisterDaily(entity);
                return new ApplicationResponse<EmployeeRegisterDaily>
                {
                    Success = true,
                    Message = "Registro diario de empleado creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<EmployeeRegisterDaily>
                {
                    Success = false,
                    Message = "Error técnico al crear el registro diario de empleado.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<EmployeeRegisterDaily> GetEmployeeRegisterDailyById(Guid id)
        {
            var entity = _employeeRegisterDailyRepository.GetEmployeeRegisterDailyById(id);
            if (entity == null)
            {
                return new ApplicationResponse<EmployeeRegisterDaily>
                {
                    Success = false,
                    Message = "Registro diario de empleado no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<EmployeeRegisterDaily>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<EmployeeRegisterDaily>> GetAllEmployeeRegisterDailies()
        {
            var entities = _employeeRegisterDailyRepository.GetAllEmployeeRegisterDailies();
            return new ApplicationResponse<IEnumerable<EmployeeRegisterDaily>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateEmployeeRegisterDaily(UpdateEmployeeRegisterDailyRequest request)
        {
            try
            {
                var before = _employeeRegisterDailyRepository.GetEmployeeRegisterDailyById(request.Id);
                var entity = new EmployeeRegisterDaily
                {
                    Id = request.Id,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _employeeRegisterDailyRepository.UpdateEmployeeRegisterDaily(entity);
                if (updated)
                {
                    _logChangeService.LogChange("EmployeeRegisterDaily", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Registro diario de empleado actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el registro diario de empleado (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el registro diario de empleado.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteEmployeeRegisterDaily(Guid id)
        {
            try
            {
                var deleted = _employeeRegisterDailyRepository.DeleteEmployeeRegisterDaily(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Registro diario de empleado eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Registro diario de empleado no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el registro diario de empleado.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<EmployeeRegisterDaily>> FindEmployeeRegisterDailiesByFields(Dictionary<string, object> filters)
        {
            var entities = _employeeRegisterDailyRepository.FindEmployeeRegisterDailiesByFields(filters);
            return new ApplicationResponse<IEnumerable<EmployeeRegisterDaily>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

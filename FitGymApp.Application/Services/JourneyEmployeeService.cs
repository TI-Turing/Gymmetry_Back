using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.JourneyEmployee.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class JourneyEmployeeService : IJourneyEmployeeService
    {
        private readonly IJourneyEmployeeRepository _journeyEmployeeRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public JourneyEmployeeService(IJourneyEmployeeRepository journeyEmployeeRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _journeyEmployeeRepository = journeyEmployeeRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<JourneyEmployee> CreateJourneyEmployee(AddJourneyEmployeeRequest request)
        {
            try
            {
                var entity = new JourneyEmployee
                {
                    Name = request.Name,
                    StartHour = request.StartHour,
                    EndHour = request.EndHour,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    EmployeeUserId = request.EmployeeUserId
                };
                var created = _journeyEmployeeRepository.CreateJourneyEmployee(entity);
                return new ApplicationResponse<JourneyEmployee>
                {
                    Success = true,
                    Message = "Jornada de empleado creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<JourneyEmployee>
                {
                    Success = false,
                    Message = "Error técnico al crear la jornada de empleado.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<JourneyEmployee> GetJourneyEmployeeById(Guid id)
        {
            var entity = _journeyEmployeeRepository.GetJourneyEmployeeById(id);
            if (entity == null)
            {
                return new ApplicationResponse<JourneyEmployee>
                {
                    Success = false,
                    Message = "Jornada de empleado no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<JourneyEmployee>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<JourneyEmployee>> GetAllJourneyEmployees()
        {
            var entities = _journeyEmployeeRepository.GetAllJourneyEmployees();
            return new ApplicationResponse<IEnumerable<JourneyEmployee>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateJourneyEmployee(UpdateJourneyEmployeeRequest request)
        {
            try
            {
                var before = _journeyEmployeeRepository.GetJourneyEmployeeById(request.Id);
                var entity = new JourneyEmployee
                {
                    Id = request.Id,
                    Name = request.Name,
                    StartHour = request.StartHour,
                    EndHour = request.EndHour,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    EmployeeUserId = request.EmployeeUserId
                };
                var updated = _journeyEmployeeRepository.UpdateJourneyEmployee(entity);
                if (updated)
                {
                    _logChangeService.LogChange("JourneyEmployee", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Jornada de empleado actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la jornada de empleado (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la jornada de empleado.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteJourneyEmployee(Guid id)
        {
            try
            {
                var deleted = _journeyEmployeeRepository.DeleteJourneyEmployee(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Jornada de empleado eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Jornada de empleado no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la jornada de empleado.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<JourneyEmployee>> FindJourneyEmployeesByFields(Dictionary<string, object> filters)
        {
            var entities = _journeyEmployeeRepository.FindJourneyEmployeesByFields(filters);
            return new ApplicationResponse<IEnumerable<JourneyEmployee>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

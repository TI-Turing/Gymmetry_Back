using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Machine.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class MachineService : IMachineService
    {
        private readonly IMachineRepository _machineRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public MachineService(IMachineRepository machineRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _machineRepository = machineRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<Machine> CreateMachine(AddMachineRequest request)
        {
            try
            {
                var entity = new Machine
                {
                    Name = request.Name,
                    MachineCategoryId = request.MachineCategoryId,
                    Ip = request.Ip
                };
                var created = _machineRepository.CreateMachine(entity);
                return new ApplicationResponse<Machine>
                {
                    Success = true,
                    Message = "Máquina creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Machine>
                {
                    Success = false,
                    Message = "Error técnico al crear la máquina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Machine> GetMachineById(Guid id)
        {
            var entity = _machineRepository.GetMachineById(id);
            if (entity == null)
            {
                return new ApplicationResponse<Machine>
                {
                    Success = false,
                    Message = "Máquina no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Machine>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<Machine>> GetAllMachines()
        {
            var entities = _machineRepository.GetAllMachines();
            return new ApplicationResponse<IEnumerable<Machine>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateMachine(UpdateMachineRequest request)
        {
            try
            {
                var before = _machineRepository.GetMachineById(request.Id);
                var entity = new Machine
                {
                    Id = request.Id,
                    Name = request.Name,
                    MachineCategoryId = request.MachineCategoryId,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _machineRepository.UpdateMachine(entity);
                if (updated)
                {
                    _logChangeService.LogChange("Machine", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Máquina actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la máquina (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la máquina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteMachine(Guid id)
        {
            try
            {
                var deleted = _machineRepository.DeleteMachine(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Máquina eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Máquina no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la máquina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Machine>> FindMachinesByFields(Dictionary<string, object> filters)
        {
            var entities = _machineRepository.FindMachinesByFields(filters);
            return new ApplicationResponse<IEnumerable<Machine>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

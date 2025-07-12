using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<ApplicationResponse<Machine>> CreateMachineAsync(AddMachineRequest request)
        {
            try
            {
                var entity = new Machine
                {
                    Name = request.Name,
                    MachineCategoryId = request.MachineCategoryId,
                    Ip = request.Ip
                };
                var created = await _machineRepository.CreateMachineAsync(entity);
                return new ApplicationResponse<Machine>
                {
                    Success = true,
                    Message = "Máquina creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<Machine>
                {
                    Success = false,
                    Message = "Error técnico al crear la máquina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<Machine>> GetMachineByIdAsync(Guid id)
        {
            var entity = await _machineRepository.GetMachineByIdAsync(id);
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

        public async Task<ApplicationResponse<IEnumerable<Machine>>> GetAllMachinesAsync()
        {
            var entities = await _machineRepository.GetAllMachinesAsync();
            return new ApplicationResponse<IEnumerable<Machine>>
            {
                Success = true,
                Data = entities
            };
        }

        public async Task<ApplicationResponse<bool>> UpdateMachineAsync(UpdateMachineRequest request)
        {
            try
            {
                var before = await _machineRepository.GetMachineByIdAsync(request.Id);
                var entity = new Machine
                {
                    Id = request.Id,
                    Name = request.Name,
                    MachineCategoryId = request.MachineCategoryId,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = await _machineRepository.UpdateMachineAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("Machine", before, entity.Id);
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
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al actualizar la máquina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteMachineAsync(Guid id)
        {
            try
            {
                var deleted = await _machineRepository.DeleteMachineAsync(id);
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
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = "Error técnico al eliminar la máquina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<Machine>>> FindMachinesByFieldsAsync(Dictionary<string, object> filters)
        {
            var entities = await _machineRepository.FindMachinesByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<Machine>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

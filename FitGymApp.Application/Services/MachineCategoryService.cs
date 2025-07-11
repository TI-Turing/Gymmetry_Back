using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.MachineCategory.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class MachineCategoryService : IMachineCategoryService
    {
        private readonly IMachineCategoryRepository _machineCategoryRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public MachineCategoryService(IMachineCategoryRepository machineCategoryRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _machineCategoryRepository = machineCategoryRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<MachineCategory> CreateMachineCategory(AddMachineCategoryRequest request)
        {
            try
            {
                var entity = new MachineCategory
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _machineCategoryRepository.CreateMachineCategory(entity);
                return new ApplicationResponse<MachineCategory>
                {
                    Success = true,
                    Message = "Categoría de máquina creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<MachineCategory>
                {
                    Success = false,
                    Message = "Error técnico al crear la categoría de máquina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<MachineCategory> GetMachineCategoryById(Guid id)
        {
            var entity = _machineCategoryRepository.GetMachineCategoryById(id);
            if (entity == null)
            {
                return new ApplicationResponse<MachineCategory>
                {
                    Success = false,
                    Message = "Categoría de máquina no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<MachineCategory>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<MachineCategory>> GetAllMachineCategories()
        {
            var entities = _machineCategoryRepository.GetAllMachineCategories();
            return new ApplicationResponse<IEnumerable<MachineCategory>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateMachineCategory(UpdateMachineCategoryRequest request)
        {
            try
            {
                var before = _machineCategoryRepository.GetMachineCategoryById(request.Id);
                var entity = new MachineCategory
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _machineCategoryRepository.UpdateMachineCategory(entity);
                if (updated)
                {
                    _logChangeService.LogChange("MachineCategory", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Categoría de máquina actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la categoría de máquina (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la categoría de máquina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteMachineCategory(Guid id)
        {
            try
            {
                var deleted = _machineCategoryRepository.DeleteMachineCategory(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Categoría de máquina eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Categoría de máquina no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la categoría de máquina.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<MachineCategory>> FindMachineCategoriesByFields(Dictionary<string, object> filters)
        {
            var entities = _machineCategoryRepository.FindMachineCategoriesByFields(filters);
            return new ApplicationResponse<IEnumerable<MachineCategory>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

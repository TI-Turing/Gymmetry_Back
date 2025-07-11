using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.GymPlanSelectedType.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class GymPlanSelectedTypeService : IGymPlanSelectedTypeService
    {
        private readonly IGymPlanSelectedTypeRepository _gymPlanSelectedTypeRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public GymPlanSelectedTypeService(IGymPlanSelectedTypeRepository gymPlanSelectedTypeRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _gymPlanSelectedTypeRepository = gymPlanSelectedTypeRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<GymPlanSelectedType> CreateGymPlanSelectedType(AddGymPlanSelectedTypeRequest request)
        {
            try
            {
                var entity = new GymPlanSelectedType
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _gymPlanSelectedTypeRepository.CreateGymPlanSelectedType(entity);
                return new ApplicationResponse<GymPlanSelectedType>
                {
                    Success = true,
                    Message = "Tipo de plan seleccionado de gimnasio creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<GymPlanSelectedType>
                {
                    Success = false,
                    Message = "Error técnico al crear el tipo de plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<GymPlanSelectedType> GetGymPlanSelectedTypeById(Guid id)
        {
            var entity = _gymPlanSelectedTypeRepository.GetGymPlanSelectedTypeById(id);
            if (entity == null)
            {
                return new ApplicationResponse<GymPlanSelectedType>
                {
                    Success = false,
                    Message = "Tipo de plan seleccionado de gimnasio no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<GymPlanSelectedType>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<GymPlanSelectedType>> GetAllGymPlanSelectedTypes()
        {
            var entities = _gymPlanSelectedTypeRepository.GetAllGymPlanSelectedTypes();
            return new ApplicationResponse<IEnumerable<GymPlanSelectedType>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateGymPlanSelectedType(UpdateGymPlanSelectedTypeRequest request)
        {
            try
            {
                var before = _gymPlanSelectedTypeRepository.GetGymPlanSelectedTypeById(request.Id);
                var entity = new GymPlanSelectedType
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _gymPlanSelectedTypeRepository.UpdateGymPlanSelectedType(entity);
                if (updated)
                {
                    _logChangeService.LogChange("GymPlanSelectedType", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de plan seleccionado de gimnasio actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el tipo de plan seleccionado de gimnasio (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el tipo de plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteGymPlanSelectedType(Guid id)
        {
            try
            {
                var deleted = _gymPlanSelectedTypeRepository.DeleteGymPlanSelectedType(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de plan seleccionado de gimnasio eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Tipo de plan seleccionado de gimnasio no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el tipo de plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<GymPlanSelectedType>> FindGymPlanSelectedTypesByFields(Dictionary<string, object> filters)
        {
            var entities = _gymPlanSelectedTypeRepository.FindGymPlanSelectedTypesByFields(filters);
            return new ApplicationResponse<IEnumerable<GymPlanSelectedType>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

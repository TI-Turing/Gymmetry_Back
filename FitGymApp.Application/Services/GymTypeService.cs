using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.GymType.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class GymTypeService : IGymTypeService
    {
        private readonly IGymTypeRepository _gymTypeRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public GymTypeService(IGymTypeRepository gymTypeRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _gymTypeRepository = gymTypeRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<GymType> CreateGymType(AddGymTypeRequest request)
        {
            try
            {
                var entity = new GymType
                {
                    Name = request.Name,
                    Ip = request.Ip
                };
                var created = _gymTypeRepository.CreateGymType(entity);
                return new ApplicationResponse<GymType>
                {
                    Success = true,
                    Message = "Tipo de gimnasio creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<GymType>
                {
                    Success = false,
                    Message = "Error técnico al crear el tipo de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<GymType> GetGymTypeById(Guid id)
        {
            var entity = _gymTypeRepository.GetGymTypeById(id);
            if (entity == null)
            {
                return new ApplicationResponse<GymType>
                {
                    Success = false,
                    Message = "Tipo de gimnasio no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<GymType>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<GymType>> GetAllGymTypes()
        {
            var entities = _gymTypeRepository.GetAllGymTypes();
            return new ApplicationResponse<IEnumerable<GymType>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateGymType(UpdateGymTypeRequest request)
        {
            try
            {
                var before = _gymTypeRepository.GetGymTypeById(request.Id);
                var entity = new GymType
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = _gymTypeRepository.UpdateGymType(entity);
                if (updated)
                {
                    _logChangeService.LogChange("GymType", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de gimnasio actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el tipo de gimnasio (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el tipo de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteGymType(Guid id)
        {
            try
            {
                var deleted = _gymTypeRepository.DeleteGymType(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Tipo de gimnasio eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Tipo de gimnasio no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el tipo de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<GymType>> FindGymTypesByFields(Dictionary<string, object> filters)
        {
            var entities = _gymTypeRepository.FindGymTypesByFields(filters);
            return new ApplicationResponse<IEnumerable<GymType>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

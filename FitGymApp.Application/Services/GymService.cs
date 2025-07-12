using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Gym.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;
using AutoMapper;

namespace FitGymApp.Application.Services
{
    public class GymService : IGymService
    {
        private readonly IGymRepository _gymRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly IMapper _mapper;

        public GymService(IGymRepository gymRepository, ILogChangeService logChangeService, ILogErrorService logErrorService, IMapper mapper)
        {
            _gymRepository = gymRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _mapper = mapper;
        }

        public ApplicationResponse<Gym> CreateGym(AddGymRequest request)
        {
            try
            {
                var entity = _mapper.Map<Gym>(request);
                var created = _gymRepository.CreateGym(entity);
                return new ApplicationResponse<Gym>
                {
                    Success = true,
                    Message = "Gimnasio creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Gym>
                {
                    Success = false,
                    Message = "Error técnico al crear el gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Gym> GetGymById(Guid id)
        {
            var entity = _gymRepository.GetGymById(id);
            if (entity == null)
            {
                return new ApplicationResponse<Gym>
                {
                    Success = false,
                    Message = "Gimnasio no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Gym>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<Gym>> GetAllGyms()
        {
            var entities = _gymRepository.GetAllGyms();
            return new ApplicationResponse<IEnumerable<Gym>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateGym(UpdateGymRequest request)
        {
            try
            {
                var before = _gymRepository.GetGymById(request.Id);
                var entity = _mapper.Map<Gym>(request);
                var updated = _gymRepository.UpdateGym(entity);
                if (updated)
                {
                    _logChangeService.LogChange("Gym", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Gimnasio actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el gimnasio (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteGym(Guid id)
        {
            try
            {
                var deleted = _gymRepository.DeleteGym(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Gimnasio eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Gimnasio no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Gym>> FindGymsByFields(Dictionary<string, object> filters)
        {
            var entities = _gymRepository.FindGymsByFields(filters);
            return new ApplicationResponse<IEnumerable<Gym>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.GymPlanSelected.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;
using AutoMapper;

namespace FitGymApp.Application.Services
{
    public class GymPlanSelectedService : IGymPlanSelectedService
    {
        private readonly IGymPlanSelectedRepository _gymPlanSelectedRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly IMapper _mapper;

        public GymPlanSelectedService(IGymPlanSelectedRepository gymPlanSelectedRepository, ILogChangeService logChangeService, ILogErrorService logErrorService, IMapper mapper)
        {
            _gymPlanSelectedRepository = gymPlanSelectedRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _mapper = mapper;
        }

        public async Task<ApplicationResponse<GymPlanSelected>> CreateGymPlanSelectedAsync(AddGymPlanSelectedRequest request)
        {
            try
            {
                var entity = _mapper.Map<GymPlanSelected>(request);
                var created = await _gymPlanSelectedRepository.CreateGymPlanSelectedAsync(entity);
                return new ApplicationResponse<GymPlanSelected>
                {
                    Success = true,
                    Message = "Plan seleccionado de gimnasio creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<GymPlanSelected>
                {
                    Success = false,
                    Message = "Error técnico al crear el plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<GymPlanSelected>> GetGymPlanSelectedByIdAsync(Guid id)
        {
            var entity = await _gymPlanSelectedRepository.GetGymPlanSelectedByIdAsync(id);
            if (entity == null)
            {
                return new ApplicationResponse<GymPlanSelected>
                {
                    Success = false,
                    Message = "Plan seleccionado de gimnasio no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<GymPlanSelected>
            {
                Success = true,
                Data = entity
            };
        }

        public async Task<ApplicationResponse<IEnumerable<GymPlanSelected>>> GetAllGymPlanSelectedsAsync()
        {
            var entities = await _gymPlanSelectedRepository.GetAllGymPlanSelectedsAsync();
            return new ApplicationResponse<IEnumerable<GymPlanSelected>>
            {
                Success = true,
                Data = entities
            };
        }

        public async Task<ApplicationResponse<bool>> UpdateGymPlanSelectedAsync(UpdateGymPlanSelectedRequest request)
        {
            try
            {
                var before = await _gymPlanSelectedRepository.GetGymPlanSelectedByIdAsync(request.Id);
                var entity = _mapper.Map<GymPlanSelected>(request);
                var updated = await _gymPlanSelectedRepository.UpdateGymPlanSelectedAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("GymPlanSelected", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Plan seleccionado de gimnasio actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el plan seleccionado de gimnasio (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteGymPlanSelectedAsync(Guid id)
        {
            try
            {
                var deleted = await _gymPlanSelectedRepository.DeleteGymPlanSelectedAsync(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Plan seleccionado de gimnasio eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Plan seleccionado de gimnasio no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el plan seleccionado de gimnasio.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<GymPlanSelected>>> FindGymPlanSelectedsByFieldsAsync(Dictionary<string, object> filters)
        {
            var entities = await _gymPlanSelectedRepository.FindGymPlanSelectedsByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<GymPlanSelected>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

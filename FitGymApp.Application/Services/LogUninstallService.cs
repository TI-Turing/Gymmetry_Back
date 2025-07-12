using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.LogUninstall.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;
using AutoMapper;

namespace FitGymApp.Application.Services
{
    public class LogUninstallService : ILogUninstallService
    {
        private readonly ILogUninstallRepository _logUninstallRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly IMapper _mapper;

        public LogUninstallService(ILogUninstallRepository logUninstallRepository, ILogChangeService logChangeService, ILogErrorService logErrorService, IMapper mapper)
        {
            _logUninstallRepository = logUninstallRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _mapper = mapper;
        }

        public ApplicationResponse<LogUninstall> CreateLogUninstall(AddLogUninstallRequest request)
        {
            try
            {
                var entity = _mapper.Map<LogUninstall>(request);
                var created = _logUninstallRepository.CreateLogUninstall(entity);
                return new ApplicationResponse<LogUninstall>
                {
                    Success = true,
                    Message = "Log de desinstalación creado correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<LogUninstall>
                {
                    Success = false,
                    Message = "Error técnico al crear el log de desinstalación.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<LogUninstall> GetLogUninstallById(Guid id)
        {
            var entity = _logUninstallRepository.GetLogUninstallById(id);
            if (entity == null)
            {
                return new ApplicationResponse<LogUninstall>
                {
                    Success = false,
                    Message = "Log de desinstalación no encontrado.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<LogUninstall>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<LogUninstall>> GetAllLogUninstalls()
        {
            var entities = _logUninstallRepository.GetAllLogUninstalls();
            return new ApplicationResponse<IEnumerable<LogUninstall>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateLogUninstall(UpdateLogUninstallRequest request)
        {
            try
            {
                var before = _logUninstallRepository.GetLogUninstallById(request.Id);
                var entity = _mapper.Map<LogUninstall>(request);
                var updated = _logUninstallRepository.UpdateLogUninstall(entity);
                if (updated)
                {
                    _logChangeService.LogChange("LogUninstall", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Log de desinstalación actualizado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar el log de desinstalación (no encontrado o inactivo).",
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
                    Message = "Error técnico al actualizar el log de desinstalación.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteLogUninstall(Guid id)
        {
            try
            {
                var deleted = _logUninstallRepository.DeleteLogUninstall(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Log de desinstalación eliminado correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Log de desinstalación no encontrado o ya eliminado.",
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
                    Message = "Error técnico al eliminar el log de desinstalación.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<LogUninstall>> FindLogUninstallsByFields(Dictionary<string, object> filters)
        {
            var entities = _logUninstallRepository.FindLogUninstallsByFields(filters);
            return new ApplicationResponse<IEnumerable<LogUninstall>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.Models;
using Gymmetry.Domain.DTO.UninstallOption.Request;
using Gymmetry.Domain.DTO;
using Gymmetry.Repository.Services.Interfaces;

namespace Gymmetry.Application.Services
{
    public class UninstallOptionService : IUninstallOptionService
    {
        private readonly IUninstallOptionRepository _uninstallOptionRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public UninstallOptionService(IUninstallOptionRepository uninstallOptionRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _uninstallOptionRepository = uninstallOptionRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public async Task<ApplicationResponse<UninstallOption>> CreateUninstallOptionAsync(AddUninstallOptionRequest request)
        {
            try
            {
                var entity = new UninstallOption
                {
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var created = await _uninstallOptionRepository.CreateUninstallOptionAsync(entity);
                return new ApplicationResponse<UninstallOption>
                {
                    Success = true,
                    Message = "Opci�n de desinstalaci�n creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<UninstallOption>
                {
                    Success = false,
                    Message = "Error t�cnico al crear la opci�n de desinstalaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<UninstallOption>> GetUninstallOptionByIdAsync(Guid id)
        {
            var entity = await _uninstallOptionRepository.GetUninstallOptionByIdAsync(id);
            if (entity == null)
            {
                return new ApplicationResponse<UninstallOption>
                {
                    Success = false,
                    Message = "Opci�n de desinstalaci�n no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<UninstallOption>
            {
                Success = true,
                Data = entity
            };
        }

        public async Task<ApplicationResponse<IEnumerable<UninstallOption>>> GetAllUninstallOptionsAsync()
        {
            var entities = await _uninstallOptionRepository.GetAllUninstallOptionsAsync();
            return new ApplicationResponse<IEnumerable<UninstallOption>>
            {
                Success = true,
                Data = entities
            };
        }

        public async Task<ApplicationResponse<bool>> UpdateUninstallOptionAsync(UpdateUninstallOptionRequest request, Guid? userId, string ip = "", string invocationId = "")
        {
            try
            {
                var before = await _uninstallOptionRepository.GetUninstallOptionByIdAsync(request.Id);
                var entity = new UninstallOption
                {
                    Id = request.Id,
                    Name = request.Name,
                    Ip = request.Ip,
                    IsActive = request.IsActive
                };
                var updated = await _uninstallOptionRepository.UpdateUninstallOptionAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("UninstallOption", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Opci�n de desinstalaci�n actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la opci�n de desinstalaci�n (no encontrada o inactiva).",
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
                    Message = "Error t�cnico al actualizar la opci�n de desinstalaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteUninstallOptionAsync(Guid id)
        {
            try
            {
                var deleted = await _uninstallOptionRepository.DeleteUninstallOptionAsync(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Opci�n de desinstalaci�n eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Opci�n de desinstalaci�n no encontrada o ya eliminada.",
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
                    Message = "Error t�cnico al eliminar la opci�n de desinstalaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<UninstallOption>>> FindUninstallOptionsByFieldsAsync(Dictionary<string, object> filters)
        {
            var entities = await _uninstallOptionRepository.FindUninstallOptionsByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<UninstallOption>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

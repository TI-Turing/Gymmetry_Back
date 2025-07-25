using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.Models;
using Gymmetry.Domain.DTO.Notification.Request;
using Gymmetry.Domain.DTO;
using Gymmetry.Repository.Services.Interfaces;
using AutoMapper;

namespace Gymmetry.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly IMapper _mapper;

        public NotificationService(INotificationRepository notificationRepository, ILogChangeService logChangeService, ILogErrorService logErrorService, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _mapper = mapper;
        }

        public async Task<ApplicationResponse<Notification>> CreateNotificationAsync(AddNotificationRequest request)
        {
            try
            {
                var entity = _mapper.Map<Notification>(request);
                var created = await _notificationRepository.CreateNotificationAsync(entity);
                return new ApplicationResponse<Notification>
                {
                    Success = true,
                    Message = "Notificaci�n creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<Notification>
                {
                    Success = false,
                    Message = "Error t�cnico al crear la notificaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<Notification>> GetNotificationByIdAsync(Guid id)
        {
            var entity = await _notificationRepository.GetNotificationByIdAsync(id);
            if (entity == null)
            {
                return new ApplicationResponse<Notification>
                {
                    Success = false,
                    Message = "Notificaci�n no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Notification>
            {
                Success = true,
                Data = entity
            };
        }

        public async Task<ApplicationResponse<IEnumerable<Notification>>> GetAllNotificationsAsync()
        {
            var entities = await _notificationRepository.GetAllNotificationsAsync();
            return new ApplicationResponse<IEnumerable<Notification>>
            {
                Success = true,
                Data = entities
            };
        }

        public async Task<ApplicationResponse<bool>> UpdateNotificationAsync(UpdateNotificationRequest request, Guid? userId, string ip = "", string invocationId = "")
        {
            try
            {
                var before = await _notificationRepository.GetNotificationByIdAsync(request.Id);
                var entity = _mapper.Map<Notification>(request);
                var updated = await _notificationRepository.UpdateNotificationAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("Notification", before, userId, ip, invocationId);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Notificaci�n actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la notificaci�n (no encontrada o inactiva).",
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
                    Message = "Error t�cnico al actualizar la notificaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteNotificationAsync(Guid id)
        {
            try
            {
                var deleted = await _notificationRepository.DeleteNotificationAsync(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Notificaci�n eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Notificaci�n no encontrada o ya eliminada.",
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
                    Message = "Error t�cnico al eliminar la notificaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<Notification>>> FindNotificationsByFieldsAsync(Dictionary<string, object> filters)
        {
            var entities = await _notificationRepository.FindNotificationsByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<Notification>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

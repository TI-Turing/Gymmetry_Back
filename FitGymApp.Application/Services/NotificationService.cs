using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Notification.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;
using AutoMapper;

namespace FitGymApp.Application.Services
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

        public ApplicationResponse<Notification> CreateNotification(AddNotificationRequest request)
        {
            try
            {
                var entity = _mapper.Map<Notification>(request);
                var created = _notificationRepository.CreateNotification(entity);
                return new ApplicationResponse<Notification>
                {
                    Success = true,
                    Message = "Notificación creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<Notification>
                {
                    Success = false,
                    Message = "Error técnico al crear la notificación.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<Notification> GetNotificationById(Guid id)
        {
            var entity = _notificationRepository.GetNotificationById(id);
            if (entity == null)
            {
                return new ApplicationResponse<Notification>
                {
                    Success = false,
                    Message = "Notificación no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<Notification>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<Notification>> GetAllNotifications()
        {
            var entities = _notificationRepository.GetAllNotifications();
            return new ApplicationResponse<IEnumerable<Notification>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateNotification(UpdateNotificationRequest request)
        {
            try
            {
                var before = _notificationRepository.GetNotificationById(request.Id);
                var entity = _mapper.Map<Notification>(request);
                var updated = _notificationRepository.UpdateNotification(entity);
                if (updated)
                {
                    _logChangeService.LogChange("Notification", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Notificación actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la notificación (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la notificación.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteNotification(Guid id)
        {
            try
            {
                var deleted = _notificationRepository.DeleteNotification(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Notificación eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Notificación no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la notificación.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<Notification>> FindNotificationsByFields(Dictionary<string, object> filters)
        {
            var entities = _notificationRepository.FindNotificationsByFields(filters);
            return new ApplicationResponse<IEnumerable<Notification>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

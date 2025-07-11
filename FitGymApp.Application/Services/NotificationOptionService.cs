using System;
using System.Collections.Generic;
using System.Linq;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.NotificationOption.Request;
using FitGymApp.Domain.DTO;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Application.Services
{
    public class NotificationOptionService : INotificationOptionService
    {
        private readonly INotificationOptionRepository _notificationOptionRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;

        public NotificationOptionService(INotificationOptionRepository notificationOptionRepository, ILogChangeService logChangeService, ILogErrorService logErrorService)
        {
            _notificationOptionRepository = notificationOptionRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
        }

        public ApplicationResponse<NotificationOption> CreateNotificationOption(AddNotificationOptionRequest request)
        {
            try
            {
                var entity = new NotificationOption
                {
                    Mail = request.Mail,
                    Push = request.Push,
                    App = request.App,
                    WhatsaApp = request.WhatsaApp,
                    Sms = request.Sms,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    UserId = request.UserId,
                    NotificationOptionNotificationNotificationOptionId = request.NotificationOptionNotificationNotificationOptionId
                };
                var created = _notificationOptionRepository.CreateNotificationOption(entity);
                return new ApplicationResponse<NotificationOption>
                {
                    Success = true,
                    Message = "Opción de notificación creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                _logErrorService.LogError(ex);
                return new ApplicationResponse<NotificationOption>
                {
                    Success = false,
                    Message = "Error técnico al crear la opción de notificación.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<NotificationOption> GetNotificationOptionById(Guid id)
        {
            var entity = _notificationOptionRepository.GetNotificationOptionById(id);
            if (entity == null)
            {
                return new ApplicationResponse<NotificationOption>
                {
                    Success = false,
                    Message = "Opción de notificación no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<NotificationOption>
            {
                Success = true,
                Data = entity
            };
        }

        public ApplicationResponse<IEnumerable<NotificationOption>> GetAllNotificationOptions()
        {
            var entities = _notificationOptionRepository.GetAllNotificationOptions();
            return new ApplicationResponse<IEnumerable<NotificationOption>>
            {
                Success = true,
                Data = entities
            };
        }

        public ApplicationResponse<bool> UpdateNotificationOption(UpdateNotificationOptionRequest request)
        {
            try
            {
                var before = _notificationOptionRepository.GetNotificationOptionById(request.Id);
                var entity = new NotificationOption
                {
                    Id = request.Id,
                    Mail = request.Mail,
                    Push = request.Push,
                    App = request.App,
                    WhatsaApp = request.WhatsaApp,
                    Sms = request.Sms,
                    Ip = request.Ip,
                    IsActive = request.IsActive,
                    UserId = request.UserId,
                    NotificationOptionNotificationNotificationOptionId = request.NotificationOptionNotificationNotificationOptionId
                };
                var updated = _notificationOptionRepository.UpdateNotificationOption(entity);
                if (updated)
                {
                    _logChangeService.LogChange("NotificationOption", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Opción de notificación actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la opción de notificación (no encontrada o inactiva).",
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
                    Message = "Error técnico al actualizar la opción de notificación.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<bool> DeleteNotificationOption(Guid id)
        {
            try
            {
                var deleted = _notificationOptionRepository.DeleteNotificationOption(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Opción de notificación eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Opción de notificación no encontrada o ya eliminada.",
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
                    Message = "Error técnico al eliminar la opción de notificación.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public ApplicationResponse<IEnumerable<NotificationOption>> FindNotificationOptionsByFields(Dictionary<string, object> filters)
        {
            var entities = _notificationOptionRepository.FindNotificationOptionsByFields(filters);
            return new ApplicationResponse<IEnumerable<NotificationOption>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

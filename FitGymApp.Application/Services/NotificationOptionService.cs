using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<ApplicationResponse<NotificationOption>> CreateNotificationOptionAsync(AddNotificationOptionRequest request)
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
                var created = await _notificationOptionRepository.CreateNotificationOptionAsync(entity);
                return new ApplicationResponse<NotificationOption>
                {
                    Success = true,
                    Message = "Opci�n de notificaci�n creada correctamente.",
                    Data = created
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<NotificationOption>
                {
                    Success = false,
                    Message = "Error t�cnico al crear la opci�n de notificaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<NotificationOption>> GetNotificationOptionByIdAsync(Guid id)
        {
            var entity = await _notificationOptionRepository.GetNotificationOptionByIdAsync(id);
            if (entity == null)
            {
                return new ApplicationResponse<NotificationOption>
                {
                    Success = false,
                    Message = "Opci�n de notificaci�n no encontrada.",
                    ErrorCode = "NotFound"
                };
            }
            return new ApplicationResponse<NotificationOption>
            {
                Success = true,
                Data = entity
            };
        }

        public async Task<ApplicationResponse<IEnumerable<NotificationOption>>> GetAllNotificationOptionsAsync()
        {
            var entities = await _notificationOptionRepository.GetAllNotificationOptionsAsync();
            return new ApplicationResponse<IEnumerable<NotificationOption>>
            {
                Success = true,
                Data = entities
            };
        }

        public async Task<ApplicationResponse<bool>> UpdateNotificationOptionAsync(UpdateNotificationOptionRequest request, Guid? userId, string ip = "", string invocationId = "")
        {
            try
            {
                var before = await _notificationOptionRepository.GetNotificationOptionByIdAsync(request.Id);
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
                var updated = await _notificationOptionRepository.UpdateNotificationOptionAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("NotificationOption", before, entity.Id);
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Opci�n de notificaci�n actualizada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "No se pudo actualizar la opci�n de notificaci�n (no encontrada o inactiva).",
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
                    Message = "Error t�cnico al actualizar la opci�n de notificaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteNotificationOptionAsync(Guid id)
        {
            try
            {
                var deleted = await _notificationOptionRepository.DeleteNotificationOptionAsync(id);
                if (deleted)
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Opci�n de notificaci�n eliminada correctamente."
                    };
                }
                else
                {
                    return new ApplicationResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Opci�n de notificaci�n no encontrada o ya eliminada.",
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
                    Message = "Error t�cnico al eliminar la opci�n de notificaci�n.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<NotificationOption>>> FindNotificationOptionsByFieldsAsync(Dictionary<string, object> filters)
        {
            var entities = await _notificationOptionRepository.FindNotificationOptionsByFieldsAsync(filters);
            return new ApplicationResponse<IEnumerable<NotificationOption>>
            {
                Success = true,
                Data = entities
            };
        }
    }
}

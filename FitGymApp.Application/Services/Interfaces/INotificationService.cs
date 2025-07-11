using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Notification.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface INotificationService
    {
        ApplicationResponse<Notification> CreateNotification(AddNotificationRequest request);
        ApplicationResponse<Notification> GetNotificationById(Guid id);
        ApplicationResponse<IEnumerable<Notification>> GetAllNotifications();
        ApplicationResponse<bool> UpdateNotification(UpdateNotificationRequest request);
        ApplicationResponse<bool> DeleteNotification(Guid id);
        ApplicationResponse<IEnumerable<Notification>> FindNotificationsByFields(Dictionary<string, object> filters);
    }
}

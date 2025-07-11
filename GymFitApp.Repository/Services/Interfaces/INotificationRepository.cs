using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface INotificationRepository
    {
        Notification CreateNotification(Notification entity);
        Notification GetNotificationById(Guid id);
        IEnumerable<Notification> GetAllNotifications();
        bool UpdateNotification(Notification entity);
        bool DeleteNotification(Guid id);
        IEnumerable<Notification> FindNotificationsByFields(Dictionary<string, object> filters);
    }
}

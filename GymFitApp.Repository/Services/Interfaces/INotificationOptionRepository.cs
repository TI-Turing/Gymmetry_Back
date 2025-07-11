using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface INotificationOptionRepository
    {
        NotificationOption CreateNotificationOption(NotificationOption entity);
        NotificationOption GetNotificationOptionById(Guid id);
        IEnumerable<NotificationOption> GetAllNotificationOptions();
        bool UpdateNotificationOption(NotificationOption entity);
        bool DeleteNotificationOption(Guid id);
        IEnumerable<NotificationOption> FindNotificationOptionsByFields(Dictionary<string, object> filters);
    }
}

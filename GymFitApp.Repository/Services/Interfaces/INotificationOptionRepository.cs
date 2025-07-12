using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface INotificationOptionRepository
    {
        Task<NotificationOption> CreateNotificationOptionAsync(NotificationOption entity);
        Task<NotificationOption?> GetNotificationOptionByIdAsync(Guid id);
        Task<IEnumerable<NotificationOption>> GetAllNotificationOptionsAsync();
        Task<bool> UpdateNotificationOptionAsync(NotificationOption entity);
        Task<bool> DeleteNotificationOptionAsync(Guid id);
        Task<IEnumerable<NotificationOption>> FindNotificationOptionsByFieldsAsync(Dictionary<string, object> filters);
    }
}

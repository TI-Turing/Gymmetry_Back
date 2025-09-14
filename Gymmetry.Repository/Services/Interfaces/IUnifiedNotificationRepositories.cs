using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface INotificationTemplateRepository
    {
        Task<NotificationTemplate> CreateAsync(NotificationTemplate template);
        Task<NotificationTemplate?> GetByIdAsync(int id);
        Task<NotificationTemplate?> GetByTemplateKeyAsync(string templateKey);
        Task<IEnumerable<NotificationTemplate>> GetAllAsync();
        Task<IEnumerable<NotificationTemplate>> GetByTypeAsync(string notificationType);
        Task<bool> UpdateAsync(NotificationTemplate template);
        Task<bool> DeleteAsync(int id);
    }

    public interface IUserNotificationPreferenceRepository
    {
        Task<UserNotificationPreference> CreateAsync(UserNotificationPreference preference);
        Task<UserNotificationPreference?> GetByUserAndTypeAsync(Guid userId, string notificationType);
        Task<IEnumerable<UserNotificationPreference>> GetByUserAsync(Guid userId);
        Task<bool> UpdateAsync(UserNotificationPreference preference);
        Task<bool> DeleteAsync(int id);
        Task<bool> CreateDefaultPreferencesAsync(Guid userId);
    }
}
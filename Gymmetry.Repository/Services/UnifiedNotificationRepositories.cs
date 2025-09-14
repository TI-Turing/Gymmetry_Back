using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;

namespace Gymmetry.Repository.Services
{
    public class NotificationTemplateRepository : INotificationTemplateRepository
    {
        private readonly GymmetryContext _context;
        private readonly ILogger<NotificationTemplateRepository> _logger;

        public NotificationTemplateRepository(GymmetryContext context, ILogger<NotificationTemplateRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<NotificationTemplate> CreateAsync(NotificationTemplate template)
        {
            template.CreatedDate = DateTime.UtcNow;
            _context.NotificationTemplates.Add(template);
            await _context.SaveChangesAsync();
            return template;
        }

        public async Task<NotificationTemplate?> GetByIdAsync(int id)
        {
            return await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        public async Task<NotificationTemplate?> GetByTemplateKeyAsync(string templateKey)
        {
            return await _context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.TemplateKey == templateKey && t.IsActive);
        }

        public async Task<IEnumerable<NotificationTemplate>> GetAllAsync()
        {
            return await _context.NotificationTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.NotificationType)
                .ThenBy(t => t.TemplateKey)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationTemplate>> GetByTypeAsync(string notificationType)
        {
            return await _context.NotificationTemplates
                .Where(t => t.NotificationType == notificationType && t.IsActive)
                .OrderBy(t => t.TemplateKey)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(NotificationTemplate template)
        {
            template.UpdatedDate = DateTime.UtcNow;
            _context.NotificationTemplates.Update(template);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var template = await GetByIdAsync(id);
            if (template == null) return false;

            template.IsActive = false;
            template.UpdatedDate = DateTime.UtcNow;
            return await UpdateAsync(template);
        }
    }

    public class UserNotificationPreferenceRepository : IUserNotificationPreferenceRepository
    {
        private readonly GymmetryContext _context;
        private readonly ILogger<UserNotificationPreferenceRepository> _logger;

        public UserNotificationPreferenceRepository(GymmetryContext context, ILogger<UserNotificationPreferenceRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserNotificationPreference> CreateAsync(UserNotificationPreference preference)
        {
            preference.CreatedDate = DateTime.UtcNow;
            _context.UserNotificationPreferences.Add(preference);
            await _context.SaveChangesAsync();
            return preference;
        }

        public async Task<UserNotificationPreference?> GetByUserAndTypeAsync(Guid userId, string notificationType)
        {
            return await _context.UserNotificationPreferences
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.NotificationType == notificationType);
        }

        public async Task<IEnumerable<UserNotificationPreference>> GetByUserAsync(Guid userId)
        {
            return await _context.UserNotificationPreferences
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.NotificationType)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(UserNotificationPreference preference)
        {
            preference.UpdatedDate = DateTime.UtcNow;
            _context.UserNotificationPreferences.Update(preference);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var preference = await _context.UserNotificationPreferences.FindAsync(id);
            if (preference == null) return false;

            _context.UserNotificationPreferences.Remove(preference);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> CreateDefaultPreferencesAsync(Guid userId)
        {
            var notificationTypes = new[] { "fitness", "social", "billing", "security", "moderation" };
            var preferences = new List<UserNotificationPreference>();

            foreach (var type in notificationTypes)
            {
                // Verificar si ya existe
                var existing = await GetByUserAndTypeAsync(userId, type);
                if (existing != null) continue;

                var preference = new UserNotificationPreference
                {
                    UserId = userId,
                    NotificationType = type,
                    PushEnabled = true,
                    AppEnabled = true,
                    EmailEnabled = type == "billing" || type == "security",
                    SmsEnabled = type == "security",
                    WhatsAppEnabled = false
                };

                preferences.Add(preference);
            }

            if (preferences.Any())
            {
                _context.UserNotificationPreferences.AddRange(preferences);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }

            return true;
        }
    }
}
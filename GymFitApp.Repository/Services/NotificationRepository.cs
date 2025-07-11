using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly FitGymAppContext _context;
        public NotificationRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Notification CreateNotification(Notification entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.Notifications.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public Notification GetNotificationById(Guid id)
        {
            return _context.Notifications.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<Notification> GetAllNotifications()
        {
            return _context.Notifications.Where(e => e.IsActive).ToList();
        }

        public bool UpdateNotification(Notification entity)
        {
            var existing = _context.Notifications.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteNotification(Guid id)
        {
            var entity = _context.Notifications.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Notification> FindNotificationsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Notification), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Notification.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Notification).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Notification, bool>>(predicate, parameter);
            return _context.Notifications.Where(lambda).ToList();
        }
    }
}

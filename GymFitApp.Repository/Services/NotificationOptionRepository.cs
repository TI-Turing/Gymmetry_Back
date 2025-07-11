using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class NotificationOptionRepository : INotificationOptionRepository
    {
        private readonly FitGymAppContext _context;
        public NotificationOptionRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public NotificationOption CreateNotificationOption(NotificationOption entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.NotificationOptions.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public NotificationOption GetNotificationOptionById(Guid id)
        {
            return _context.NotificationOptions.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<NotificationOption> GetAllNotificationOptions()
        {
            return _context.NotificationOptions.Where(e => e.IsActive).ToList();
        }

        public bool UpdateNotificationOption(NotificationOption entity)
        {
            var existing = _context.NotificationOptions.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteNotificationOption(Guid id)
        {
            var entity = _context.NotificationOptions.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<NotificationOption> FindNotificationOptionsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(NotificationOption), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(NotificationOption.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(NotificationOption).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<NotificationOption, bool>>(predicate, parameter);
            return _context.NotificationOptions.Where(lambda).ToList();
        }
    }
}

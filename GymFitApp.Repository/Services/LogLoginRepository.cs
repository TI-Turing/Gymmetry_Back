using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class LogLoginRepository : ILogLoginRepository
    {
        private readonly FitGymAppContext _context;
        public LogLoginRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public LogLogin CreateLogLogin(LogLogin entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.LogLogins.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public LogLogin GetLogLoginById(Guid id)
        {
            return _context.LogLogins.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<LogLogin> GetAllLogLogins()
        {
            return _context.LogLogins.Where(e => e.IsActive).ToList();
        }

        public bool UpdateLogLogin(LogLogin entity)
        {
            var existing = _context.LogLogins.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteLogLogin(Guid id)
        {
            var entity = _context.LogLogins.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<LogLogin> FindLogLoginsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(LogLogin), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(LogLogin.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(LogLogin).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<LogLogin, bool>>(predicate, parameter);
            return _context.LogLogins.Where(lambda).ToList();
        }
    }
}

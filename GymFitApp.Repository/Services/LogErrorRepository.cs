using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class LogErrorRepository : ILogErrorRepository
    {
        private readonly FitGymAppContext _context;
        public LogErrorRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public LogError CreateLogError(LogError entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.LogErrors.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public LogError GetLogErrorById(Guid id)
        {
            return _context.LogErrors.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<LogError> GetAllLogErrors()
        {
            return _context.LogErrors.Where(e => e.IsActive).ToList();
        }

        public bool UpdateLogError(LogError entity)
        {
            var existing = _context.LogErrors.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteLogError(Guid id)
        {
            var entity = _context.LogErrors.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<LogError> FindLogErrorsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(LogError), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(LogError.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(LogError).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<LogError, bool>>(predicate, parameter);
            return _context.LogErrors.Where(lambda).ToList();
        }
    }
}

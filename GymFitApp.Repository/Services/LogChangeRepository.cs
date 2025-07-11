using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class LogChangeRepository : ILogChangeRepository
    {
        private readonly FitGymAppContext _context;
        public LogChangeRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public LogChange CreateLogChange(LogChange entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.LogChanges.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public LogChange GetLogChangeById(Guid id)
        {
            return _context.LogChanges.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<LogChange> GetAllLogChanges()
        {
            return _context.LogChanges.Where(e => e.IsActive).ToList();
        }

        public bool UpdateLogChange(LogChange entity)
        {
            var existing = _context.LogChanges.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteLogChange(Guid id)
        {
            var entity = _context.LogChanges.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<LogChange> FindLogChangesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(LogChange), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(LogChange.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(LogChange).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<LogChange, bool>>(predicate, parameter);
            return _context.LogChanges.Where(lambda).ToList();
        }
    }
}

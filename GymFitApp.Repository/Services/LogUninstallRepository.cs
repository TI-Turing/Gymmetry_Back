using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class LogUninstallRepository : ILogUninstallRepository
    {
        private readonly FitGymAppContext _context;
        public LogUninstallRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public LogUninstall CreateLogUninstall(LogUninstall entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.LogUninstalls.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public LogUninstall GetLogUninstallById(Guid id)
        {
            return _context.LogUninstalls.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<LogUninstall> GetAllLogUninstalls()
        {
            return _context.LogUninstalls.Where(e => e.IsActive).ToList();
        }

        public bool UpdateLogUninstall(LogUninstall entity)
        {
            var existing = _context.LogUninstalls.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteLogUninstall(Guid id)
        {
            var entity = _context.LogUninstalls.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<LogUninstall> FindLogUninstallsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(LogUninstall), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(LogUninstall.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(LogUninstall).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<LogUninstall, bool>>(predicate, parameter);
            return _context.LogUninstalls.Where(lambda).ToList();
        }
    }
}

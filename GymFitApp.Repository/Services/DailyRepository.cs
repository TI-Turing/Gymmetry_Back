using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class DailyRepository : IDailyRepository
    {
        private readonly FitGymAppContext _context;
        public DailyRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Daily CreateDaily(Daily entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.Dailies.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public Daily GetDailyById(Guid id)
        {
            return _context.Dailies.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<Daily> GetAllDailies()
        {
            return _context.Dailies.Where(e => e.IsActive).ToList();
        }

        public bool UpdateDaily(Daily entity)
        {
            var existing = _context.Dailies.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteDaily(Guid id)
        {
            var entity = _context.Dailies.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Daily> FindDailiesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Daily), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Daily.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Daily).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Daily, bool>>(predicate, parameter);
            return _context.Dailies.Where(lambda).ToList();
        }
    }
}

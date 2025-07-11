using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class DailyHistoryRepository : IDailyHistoryRepository
    {
        private readonly FitGymAppContext _context;
        public DailyHistoryRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public DailyHistory CreateDailyHistory(DailyHistory entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.DailyHistories.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public DailyHistory GetDailyHistoryById(Guid id)
        {
            return _context.DailyHistories.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<DailyHistory> GetAllDailyHistories()
        {
            return _context.DailyHistories.Where(e => e.IsActive).ToList();
        }

        public bool UpdateDailyHistory(DailyHistory entity)
        {
            var existing = _context.DailyHistories.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteDailyHistory(Guid id)
        {
            var entity = _context.DailyHistories.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<DailyHistory> FindDailyHistoriesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(DailyHistory), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(DailyHistory.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(DailyHistory).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<DailyHistory, bool>>(predicate, parameter);
            return _context.DailyHistories.Where(lambda).ToList();
        }
    }
}

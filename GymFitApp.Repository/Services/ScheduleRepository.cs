using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly FitGymAppContext _context;
        public ScheduleRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Schedule CreateSchedule(Schedule entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.Schedules.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public Schedule GetScheduleById(Guid id)
        {
            return _context.Schedules.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<Schedule> GetAllSchedules()
        {
            return _context.Schedules.Where(e => e.IsActive).ToList();
        }

        public bool UpdateSchedule(Schedule entity)
        {
            var existing = _context.Schedules.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteSchedule(Guid id)
        {
            var entity = _context.Schedules.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Schedule> FindSchedulesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Schedule), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Schedule.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Schedule).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Schedule, bool>>(predicate, parameter);
            return _context.Schedules.Where(lambda).ToList();
        }
    }
}

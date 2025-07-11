using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class DailyExerciseRepository : IDailyExerciseRepository
    {
        private readonly FitGymAppContext _context;
        public DailyExerciseRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public DailyExercise CreateDailyExercise(DailyExercise entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.DailyExercises.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public DailyExercise GetDailyExerciseById(Guid id)
        {
            return _context.DailyExercises.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<DailyExercise> GetAllDailyExercises()
        {
            return _context.DailyExercises.Where(e => e.IsActive).ToList();
        }

        public bool UpdateDailyExercise(DailyExercise entity)
        {
            var existing = _context.DailyExercises.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteDailyExercise(Guid id)
        {
            var entity = _context.DailyExercises.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<DailyExercise> FindDailyExercisesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(DailyExercise), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(DailyExercise.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(DailyExercise).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<DailyExercise, bool>>(predicate, parameter);
            return _context.DailyExercises.Where(lambda).ToList();
        }
    }
}

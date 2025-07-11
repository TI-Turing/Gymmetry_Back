using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class RoutineExerciseRepository : IRoutineExerciseRepository
    {
        private readonly FitGymAppContext _context;
        public RoutineExerciseRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public RoutineExercise CreateRoutineExercise(RoutineExercise entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.RoutineExercises.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public RoutineExercise GetRoutineExerciseById(Guid id)
        {
            return _context.RoutineExercises.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<RoutineExercise> GetAllRoutineExercises()
        {
            return _context.RoutineExercises.Where(e => e.IsActive).ToList();
        }

        public bool UpdateRoutineExercise(RoutineExercise entity)
        {
            var existing = _context.RoutineExercises.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteRoutineExercise(Guid id)
        {
            var entity = _context.RoutineExercises.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<RoutineExercise> FindRoutineExercisesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(RoutineExercise), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(RoutineExercise.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(RoutineExercise).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<RoutineExercise, bool>>(predicate, parameter);
            return _context.RoutineExercises.Where(lambda).ToList();
        }
    }
}

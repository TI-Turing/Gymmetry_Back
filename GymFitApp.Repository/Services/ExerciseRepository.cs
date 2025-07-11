using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly FitGymAppContext _context;
        public ExerciseRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Exercise CreateExercise(Exercise entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.Exercises.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public Exercise GetExerciseById(Guid id)
        {
            return _context.Exercises.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<Exercise> GetAllExercises()
        {
            return _context.Exercises.Where(e => e.IsActive).ToList();
        }

        public bool UpdateExercise(Exercise entity)
        {
            var existing = _context.Exercises.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteExercise(Guid id)
        {
            var entity = _context.Exercises.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Exercise> FindExercisesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Exercise), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Exercise.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Exercise).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Exercise, bool>>(predicate, parameter);
            return _context.Exercises.Where(lambda).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class CategoryExerciseRepository : ICategoryExerciseRepository
    {
        private readonly FitGymAppContext _context;
        public CategoryExerciseRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public CategoryExercise CreateCategoryExercise(CategoryExercise entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.CategoryExercises.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public CategoryExercise GetCategoryExerciseById(Guid id)
        {
            return _context.CategoryExercises.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<CategoryExercise> GetAllCategoryExercises()
        {
            return _context.CategoryExercises.Where(e => e.IsActive).ToList();
        }

        public bool UpdateCategoryExercise(CategoryExercise entity)
        {
            var existing = _context.CategoryExercises.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteCategoryExercise(Guid id)
        {
            var entity = _context.CategoryExercises.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<CategoryExercise> FindCategoryExercisesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(CategoryExercise), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(CategoryExercise.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(CategoryExercise).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<CategoryExercise, bool>>(predicate, parameter);
            return _context.CategoryExercises.Where(lambda).ToList();
        }
    }
}

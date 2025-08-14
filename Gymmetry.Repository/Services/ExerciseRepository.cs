using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gymmetry.Repository.Services
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly GymmetryContext _context;
        public ExerciseRepository(GymmetryContext context)
        {
            _context = context;
        }

        public async Task<Exercise> CreateExerciseAsync(Exercise entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.Exercises.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Exercise?> GetExerciseByIdAsync(Guid id)
        {
            var exercise = await _context.Exercises
                .Where(e => e.Id == id && e.IsActive)
                .Select(e => new {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.CategoryExerciseId,
                    e.TagsMuscle,
                    e.TagsObjectives,
                    CategoryExercise = new {
                        e.CategoryExercise.Id,
                        e.CategoryExercise.Name
                    }
                })
                .FirstOrDefaultAsync();

            return exercise != null ? new Exercise {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description,
                CategoryExerciseId = exercise.CategoryExerciseId,
                TagsMuscle=exercise.TagsMuscle,
                TagsObjectives=exercise.TagsObjectives,
                CategoryExercise = new CategoryExercise {
                    Id = exercise.CategoryExercise.Id,
                    Name = exercise.CategoryExercise.Name
                }
            } : null;
        }

        public async Task<IEnumerable<Exercise>> GetAllExercisesAsync()
        {
            return await _context.Exercises.Include(x => x.CategoryExercise).Where(e => e.IsActive).ToListAsync();
        }

        public async Task<bool> UpdateExerciseAsync(Exercise entity)
        {
            var existing = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteExerciseAsync(Guid id)
        {
            var entity = await _context.Exercises.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Exercise>> FindExercisesByFieldsAsync(Dictionary<string, object> filters)
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
                var value = ValueConverter.ConvertValueToType(filter.Value, property.PropertyType);
                Expression containsExpr;
                if (property.PropertyType == typeof(string))
                {
                    containsExpr = Expression.Call(left, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, Expression.Constant(value));
                }
                else
                {
                    containsExpr = Expression.Equal(left, Expression.Constant(value));
                }
                predicate = Expression.AndAlso(predicate, containsExpr);
            }
            var lambda = Expression.Lambda<Func<Exercise, bool>>(predicate, parameter);
            var exercises = await _context.Exercises
                .Where(lambda)
                .Select(e => new Exercise {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    TagsObjectives = e.TagsObjectives,
                    TagsMuscle = e.TagsMuscle,
                    CategoryExerciseId = e.CategoryExerciseId,
                    CategoryExercise = e.CategoryExercise == null ? null : new CategoryExercise {
                        Id = e.CategoryExercise.Id,
                        Name = e.CategoryExercise.Name
                    }
                })
                .ToListAsync();
            return exercises;
        }
    }
}

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
    public class DailyExerciseRepository : IDailyExerciseRepository
    {
        private readonly GymmetryContext _context;
        public DailyExerciseRepository(GymmetryContext context)
        {
            _context = context;
        }

        public async Task<DailyExercise> CreateDailyExerciseAsync(DailyExercise entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.DailyExercises.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<DailyExercise?> GetDailyExerciseByIdAsync(Guid id)
        {
            return await _context.DailyExercises
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        }

        public async Task<IEnumerable<DailyExercise>> GetAllDailyExercisesAsync()
        {
            return await _context.DailyExercises
                .Where(e => e.IsActive)
                .ToListAsync();
        }

        public async Task<bool> UpdateDailyExerciseAsync(DailyExercise entity)
        {
            var existing = await _context.DailyExercises
                .FirstOrDefaultAsync(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteDailyExerciseAsync(Guid id)
        {
            var entity = await _context.DailyExercises
                .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<DailyExercise>> FindDailyExercisesByFieldsAsync(Dictionary<string, object> filters)
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
                var right = Expression.Constant(ValueConverter.ConvertValueToType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }

            var lambda = Expression.Lambda<Func<DailyExercise, bool>>(predicate, parameter);
            return await _context.DailyExercises.Where(lambda).ToListAsync();
        }

        public async Task<IEnumerable<DailyExercise>> CreateDailyExercisesBulkAsync(IEnumerable<DailyExercise> entities)
        {
            var list = entities.ToList();
            foreach (var entity in list)
            {
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.IsActive = true;
                _context.DailyExercises.Add(entity);
            }
            await _context.SaveChangesAsync();
            return list;
        }

        public async Task<IReadOnlyList<DailyExercise>> GetByDailyIdsAsync(IEnumerable<Guid> dailyIds)
        {
            var ids = dailyIds.ToList();
            if (!ids.Any()) return new List<DailyExercise>();
            return await _context.DailyExercises
                .AsNoTracking()
                .Include(x => x.Exercise)
                .Where(x => x.IsActive && ids.Contains(x.DailyId))
                .ToListAsync();
        }
    }
}

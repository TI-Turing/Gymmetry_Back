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
    public class UserExerciseMaxRepository : IUserExerciseMaxRepository
    {
        private readonly GymmetryContext _context;
        public UserExerciseMaxRepository(GymmetryContext context) => _context = context;

        public async Task<UserExerciseMax> CreateAsync(UserExerciseMax entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.UserExerciseMaxes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<UserExerciseMax?> GetByIdAsync(Guid id)
        {
            return await _context.UserExerciseMaxes.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        }

        public async Task<IEnumerable<UserExerciseMax>> GetAllAsync()
        {
            return await _context.UserExerciseMaxes.Where(e => e.IsActive).ToListAsync();
        }

        public async Task<bool> UpdateAsync(UserExerciseMax entity)
        {
            var existing = await _context.UserExerciseMaxes.FirstOrDefaultAsync(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.UserExerciseMaxes.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<UserExerciseMax>> FindByFieldsAsync(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(UserExerciseMax), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(UserExerciseMax.IsActive)),
                Expression.Constant(true)
            );

            foreach (var filter in filters)
            {
                var property = typeof(UserExerciseMax).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(ValueConverter.ConvertValueToType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }

            var lambda = Expression.Lambda<Func<UserExerciseMax, bool>>(predicate, parameter);
            return await _context.UserExerciseMaxes.Where(lambda).ToListAsync();
        }

        public async Task<IEnumerable<UserExerciseMax>> GetLatestByUserAsync(Guid userId, int topExercises = 10)
        {
            // Obtener último registro (por AchievedAt) por ejercicio para el usuario
            var query = _context.UserExerciseMaxes
                .Where(x => x.UserId == userId && x.IsActive)
                .GroupBy(x => x.ExerciseId)
                .Select(g => g.OrderByDescending(e => e.AchievedAt).First())
                .OrderByDescending(x => x.AchievedAt)
                .Take(topExercises)
                .Include(x => x.Exercise);
            return await query.ToListAsync();
        }
    }
}

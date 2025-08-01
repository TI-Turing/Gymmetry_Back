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
    public class EmployeeTypeRepository : IEmployeeTypeRepository
    {
        private readonly GymmetryContext _context;
        public EmployeeTypeRepository(GymmetryContext context)
        {
            _context = context;
        }

        public async Task<EmployeeType> CreateEmployeeTypeAsync(EmployeeType entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.EmployeeTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<EmployeeType?> GetEmployeeTypeByIdAsync(Guid id)
        {
            return await _context.EmployeeTypes.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        }

        public async Task<IEnumerable<EmployeeType>> GetAllEmployeeTypesAsync()
        {
            return await _context.EmployeeTypes.Where(e => e.IsActive).ToListAsync();
        }

        public async Task<bool> UpdateEmployeeTypeAsync(EmployeeType entity)
        {
            var existing = await _context.EmployeeTypes.FirstOrDefaultAsync(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteEmployeeTypeAsync(Guid id)
        {
            var entity = await _context.EmployeeTypes.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<EmployeeType>> FindEmployeeTypesByFieldsAsync(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(EmployeeType), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(EmployeeType.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(EmployeeType).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(ValueConverter.ConvertValueToType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<EmployeeType, bool>>(predicate, parameter);
            return await _context.EmployeeTypes.Where(lambda).ToListAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class EmployeeTypeRepository : IEmployeeTypeRepository
    {
        private readonly FitGymAppContext _context;
        public EmployeeTypeRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public EmployeeType CreateEmployeeType(EmployeeType entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.EmployeeTypes.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public EmployeeType GetEmployeeTypeById(Guid id)
        {
            return _context.EmployeeTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<EmployeeType> GetAllEmployeeTypes()
        {
            return _context.EmployeeTypes.Where(e => e.IsActive).ToList();
        }

        public bool UpdateEmployeeType(EmployeeType entity)
        {
            var existing = _context.EmployeeTypes.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteEmployeeType(Guid id)
        {
            var entity = _context.EmployeeTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<EmployeeType> FindEmployeeTypesByFields(Dictionary<string, object> filters)
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
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<EmployeeType, bool>>(predicate, parameter);
            return _context.EmployeeTypes.Where(lambda).ToList();
        }
    }
}

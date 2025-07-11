using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class EmployeeRegisterDailyRepository : IEmployeeRegisterDailyRepository
    {
        private readonly FitGymAppContext _context;
        public EmployeeRegisterDailyRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public EmployeeRegisterDaily CreateEmployeeRegisterDaily(EmployeeRegisterDaily entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.EmployeeRegisterDailies.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public EmployeeRegisterDaily GetEmployeeRegisterDailyById(Guid id)
        {
            return _context.EmployeeRegisterDailies.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<EmployeeRegisterDaily> GetAllEmployeeRegisterDailies()
        {
            return _context.EmployeeRegisterDailies.Where(e => e.IsActive).ToList();
        }

        public bool UpdateEmployeeRegisterDaily(EmployeeRegisterDaily entity)
        {
            var existing = _context.EmployeeRegisterDailies.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteEmployeeRegisterDaily(Guid id)
        {
            var entity = _context.EmployeeRegisterDailies.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<EmployeeRegisterDaily> FindEmployeeRegisterDailiesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(EmployeeRegisterDaily), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(EmployeeRegisterDaily.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(EmployeeRegisterDaily).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<EmployeeRegisterDaily, bool>>(predicate, parameter);
            return _context.EmployeeRegisterDailies.Where(lambda).ToList();
        }
    }
}

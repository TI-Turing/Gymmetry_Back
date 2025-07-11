using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class JourneyEmployeeRepository : IJourneyEmployeeRepository
    {
        private readonly FitGymAppContext _context;
        public JourneyEmployeeRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public JourneyEmployee CreateJourneyEmployee(JourneyEmployee entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.JourneyEmployees.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public JourneyEmployee GetJourneyEmployeeById(Guid id)
        {
            return _context.JourneyEmployees.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<JourneyEmployee> GetAllJourneyEmployees()
        {
            return _context.JourneyEmployees.Where(e => e.IsActive).ToList();
        }

        public bool UpdateJourneyEmployee(JourneyEmployee entity)
        {
            var existing = _context.JourneyEmployees.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteJourneyEmployee(Guid id)
        {
            var entity = _context.JourneyEmployees.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<JourneyEmployee> FindJourneyEmployeesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(JourneyEmployee), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(JourneyEmployee.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(JourneyEmployee).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<JourneyEmployee, bool>>(predicate, parameter);
            return _context.JourneyEmployees.Where(lambda).ToList();
        }
    }
}

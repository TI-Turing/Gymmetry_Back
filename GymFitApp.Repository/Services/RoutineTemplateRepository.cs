using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class RoutineTemplateRepository : IRoutineTemplateRepository
    {
        private readonly FitGymAppContext _context;
        public RoutineTemplateRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public RoutineTemplate CreateRoutineTemplate(RoutineTemplate entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.RoutineTemplates.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public RoutineTemplate GetRoutineTemplateById(Guid id)
        {
            return _context.RoutineTemplates.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<RoutineTemplate> GetAllRoutineTemplates()
        {
            return _context.RoutineTemplates.Where(e => e.IsActive).ToList();
        }

        public bool UpdateRoutineTemplate(RoutineTemplate entity)
        {
            var existing = _context.RoutineTemplates.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteRoutineTemplate(Guid id)
        {
            var entity = _context.RoutineTemplates.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<RoutineTemplate> FindRoutineTemplatesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(RoutineTemplate), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(RoutineTemplate.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(RoutineTemplate).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<RoutineTemplate, bool>>(predicate, parameter);
            return _context.RoutineTemplates.Where(lambda).ToList();
        }
    }
}

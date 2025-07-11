using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class SubModuleRepository : ISubModuleRepository
    {
        private readonly FitGymAppContext _context;
        public SubModuleRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public SubModule CreateSubModule(SubModule entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.SubModules.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public SubModule GetSubModuleById(Guid id)
        {
            return _context.SubModules.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<SubModule> GetAllSubModules()
        {
            return _context.SubModules.Where(e => e.IsActive).ToList();
        }

        public bool UpdateSubModule(SubModule entity)
        {
            var existing = _context.SubModules.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteSubModule(Guid id)
        {
            var entity = _context.SubModules.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<SubModule> FindSubModulesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(SubModule), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(SubModule.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(SubModule).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<SubModule, bool>>(predicate, parameter);
            return _context.SubModules.Where(lambda).ToList();
        }
    }
}

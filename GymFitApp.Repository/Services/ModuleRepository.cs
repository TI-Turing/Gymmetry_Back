using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly FitGymAppContext _context;
        public ModuleRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Module CreateModule(Module entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.Modules.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public Module GetModuleById(Guid id)
        {
            return _context.Modules.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<Module> GetAllModules()
        {
            return _context.Modules.Where(e => e.IsActive).ToList();
        }

        public bool UpdateModule(Module entity)
        {
            var existing = _context.Modules.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteModule(Guid id)
        {
            var entity = _context.Modules.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Module> FindModulesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Module), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Module.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Module).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Module, bool>>(predicate, parameter);
            return _context.Modules.Where(lambda).ToList();
        }
    }
}

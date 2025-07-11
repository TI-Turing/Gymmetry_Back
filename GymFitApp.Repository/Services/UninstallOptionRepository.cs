using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class UninstallOptionRepository : IUninstallOptionRepository
    {
        private readonly FitGymAppContext _context;
        public UninstallOptionRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public UninstallOption CreateUninstallOption(UninstallOption entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.UninstallOptions.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public UninstallOption GetUninstallOptionById(Guid id)
        {
            return _context.UninstallOptions.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<UninstallOption> GetAllUninstallOptions()
        {
            return _context.UninstallOptions.Where(e => e.IsActive).ToList();
        }

        public bool UpdateUninstallOption(UninstallOption entity)
        {
            var existing = _context.UninstallOptions.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteUninstallOption(Guid id)
        {
            var entity = _context.UninstallOptions.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<UninstallOption> FindUninstallOptionsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(UninstallOption), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(UninstallOption.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(UninstallOption).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<UninstallOption, bool>>(predicate, parameter);
            return _context.UninstallOptions.Where(lambda).ToList();
        }
    }
}

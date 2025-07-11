using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly FitGymAppContext _context;
        public PermissionRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Permission CreatePermission(Permission entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.Permissions.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public Permission GetPermissionById(Guid id)
        {
            return _context.Permissions.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<Permission> GetAllPermissions()
        {
            return _context.Permissions.Where(e => e.IsActive).ToList();
        }

        public bool UpdatePermission(Permission entity)
        {
            var existing = _context.Permissions.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeletePermission(Guid id)
        {
            var entity = _context.Permissions.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Permission> FindPermissionsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Permission), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Permission.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Permission).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Permission, bool>>(predicate, parameter);
            return _context.Permissions.Where(lambda).ToList();
        }
    }
}

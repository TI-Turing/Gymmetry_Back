using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class AccessMethodTypeRepository : IAccessMethodTypeRepository
    {
        private readonly FitGymAppContext _context;
        public AccessMethodTypeRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public AccessMethodType CreateAccessMethodType(AccessMethodType entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.AccessMethodTypes.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public AccessMethodType GetAccessMethodTypeById(Guid id)
        {
            return _context.AccessMethodTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<AccessMethodType> GetAllAccessMethodTypes()
        {
            return _context.AccessMethodTypes.Where(e => e.IsActive).ToList();
        }

        public bool UpdateAccessMethodType(AccessMethodType entity)
        {
            var existing = _context.AccessMethodTypes.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteAccessMethodType(Guid id)
        {
            var entity = _context.AccessMethodTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<AccessMethodType> FindAccessMethodTypesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(AccessMethodType), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(AccessMethodType.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(AccessMethodType).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<AccessMethodType, bool>>(predicate, parameter);
            return _context.AccessMethodTypes.Where(lambda).ToList();
        }
    }
}

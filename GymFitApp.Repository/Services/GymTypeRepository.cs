using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class GymTypeRepository : IGymTypeRepository
    {
        private readonly FitGymAppContext _context;
        public GymTypeRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public GymType CreateGymType(GymType entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.GymTypes.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public GymType GetGymTypeById(Guid id)
        {
            return _context.GymTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<GymType> GetAllGymTypes()
        {
            return _context.GymTypes.Where(e => e.IsActive).ToList();
        }

        public bool UpdateGymType(GymType entity)
        {
            var existing = _context.GymTypes.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteGymType(Guid id)
        {
            var entity = _context.GymTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<GymType> FindGymTypesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(GymType), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(GymType.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(GymType).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<GymType, bool>>(predicate, parameter);
            return _context.GymTypes.Where(lambda).ToList();
        }
    }
}

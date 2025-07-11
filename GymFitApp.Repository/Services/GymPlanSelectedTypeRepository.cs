using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class GymPlanSelectedTypeRepository : IGymPlanSelectedTypeRepository
    {
        private readonly FitGymAppContext _context;
        public GymPlanSelectedTypeRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public GymPlanSelectedType CreateGymPlanSelectedType(GymPlanSelectedType entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.GymPlanSelectedTypes.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public GymPlanSelectedType GetGymPlanSelectedTypeById(Guid id)
        {
            return _context.GymPlanSelectedTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<GymPlanSelectedType> GetAllGymPlanSelectedTypes()
        {
            return _context.GymPlanSelectedTypes.Where(e => e.IsActive).ToList();
        }

        public bool UpdateGymPlanSelectedType(GymPlanSelectedType entity)
        {
            var existing = _context.GymPlanSelectedTypes.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteGymPlanSelectedType(Guid id)
        {
            var entity = _context.GymPlanSelectedTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<GymPlanSelectedType> FindGymPlanSelectedTypesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(GymPlanSelectedType), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(GymPlanSelectedType.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(GymPlanSelectedType).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<GymPlanSelectedType, bool>>(predicate, parameter);
            return _context.GymPlanSelectedTypes.Where(lambda).ToList();
        }
    }
}

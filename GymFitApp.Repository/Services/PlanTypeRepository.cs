using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class PlanTypeRepository : IPlanTypeRepository
    {
        private readonly FitGymAppContext _context;
        public PlanTypeRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public PlanType CreatePlanType(PlanType entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.PlanTypes.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public PlanType GetPlanTypeById(Guid id)
        {
            return _context.PlanTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<PlanType> GetAllPlanTypes()
        {
            return _context.PlanTypes.Where(e => e.IsActive).ToList();
        }

        public bool UpdatePlanType(PlanType entity)
        {
            var existing = _context.PlanTypes.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeletePlanType(Guid id)
        {
            var entity = _context.PlanTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<PlanType> FindPlanTypesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(PlanType), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(PlanType.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(PlanType).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<PlanType, bool>>(predicate, parameter);
            return _context.PlanTypes.Where(lambda).ToList();
        }
    }
}

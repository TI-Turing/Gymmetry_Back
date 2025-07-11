using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class PlanRepository : IPlanRepository
    {
        private readonly FitGymAppContext _context;
        public PlanRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Plan CreatePlan(Plan plan)
        {
            plan.Id = Guid.NewGuid();
            plan.CreatedAt = DateTime.UtcNow;
            plan.IsActive = true;
            _context.Plans.Add(plan);
            _context.SaveChanges();
            return plan;
        }

        public Plan GetPlanById(Guid id)
        {
            return _context.Plans.FirstOrDefault(p => p.Id == id && p.IsActive);
        }

        public IEnumerable<Plan> GetAllPlans()
        {
            return _context.Plans.Where(p => p.IsActive).ToList();
        }

        public bool UpdatePlan(Plan plan)
        {
            var existing = _context.Plans.FirstOrDefault(p => p.Id == plan.Id && p.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(plan);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeletePlan(Guid id)
        {
            var entity = _context.Plans.FirstOrDefault(p => p.Id == id && p.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Plan> FindPlansByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Plan), "p");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Plan.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Plan).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Plan, bool>>(predicate, parameter);
            return _context.Plans.Where(lambda).ToList();
        }
    }
}

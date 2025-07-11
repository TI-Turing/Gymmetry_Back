using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class GymPlanSelectedRepository : IGymPlanSelectedRepository
    {
        private readonly FitGymAppContext _context;
        public GymPlanSelectedRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public GymPlanSelected CreateGymPlanSelected(GymPlanSelected entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.GymPlanSelecteds.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public GymPlanSelected GetGymPlanSelectedById(Guid id)
        {
            return _context.GymPlanSelecteds.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<GymPlanSelected> GetAllGymPlanSelecteds()
        {
            return _context.GymPlanSelecteds.Where(e => e.IsActive).ToList();
        }

        public bool UpdateGymPlanSelected(GymPlanSelected entity)
        {
            var existing = _context.GymPlanSelecteds.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteGymPlanSelected(Guid id)
        {
            var entity = _context.GymPlanSelecteds.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<GymPlanSelected> FindGymPlanSelectedsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(GymPlanSelected), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(GymPlanSelected.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(GymPlanSelected).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<GymPlanSelected, bool>>(predicate, parameter);
            return _context.GymPlanSelecteds.Where(lambda).ToList();
        }
    }
}

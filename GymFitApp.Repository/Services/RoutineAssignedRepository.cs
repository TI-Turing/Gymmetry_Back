using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class RoutineAssignedRepository : IRoutineAssignedRepository
    {
        private readonly FitGymAppContext _context;
        public RoutineAssignedRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public RoutineAssigned CreateRoutineAssigned(RoutineAssigned entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.RoutineAssigneds.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public RoutineAssigned GetRoutineAssignedById(Guid id)
        {
            return _context.RoutineAssigneds.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<RoutineAssigned> GetAllRoutineAssigneds()
        {
            return _context.RoutineAssigneds.Where(e => e.IsActive).ToList();
        }

        public bool UpdateRoutineAssigned(RoutineAssigned entity)
        {
            var existing = _context.RoutineAssigneds.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteRoutineAssigned(Guid id)
        {
            var entity = _context.RoutineAssigneds.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<RoutineAssigned> FindRoutineAssignedsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(RoutineAssigned), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(RoutineAssigned.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(RoutineAssigned).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<RoutineAssigned, bool>>(predicate, parameter);
            return _context.RoutineAssigneds.Where(lambda).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class GymRepository : IGymRepository
    {
        private readonly FitGymAppContext _context;
        public GymRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Gym CreateGym(Gym entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.Gyms.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public Gym GetGymById(Guid id)
        {
            return _context.Gyms.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<Gym> GetAllGyms()
        {
            return _context.Gyms.Where(e => e.IsActive).ToList();
        }

        public bool UpdateGym(Gym entity)
        {
            var existing = _context.Gyms.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteGym(Guid id)
        {
            var entity = _context.Gyms.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Gym> FindGymsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Gym), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Gym.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Gym).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Gym, bool>>(predicate, parameter);
            return _context.Gyms.Where(lambda).ToList();
        }
    }
}

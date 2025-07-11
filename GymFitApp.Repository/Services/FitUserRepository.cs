using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class FitUserRepository : IFitUserRepository
    {
        private readonly FitGymAppContext _context;
        public FitUserRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public FitUser CreateFitUser(FitUser entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.FitUsers.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public FitUser GetFitUserById(Guid id)
        {
            return _context.FitUsers.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<FitUser> GetAllFitUsers()
        {
            return _context.FitUsers.Where(e => e.IsActive).ToList();
        }

        public bool UpdateFitUser(FitUser entity)
        {
            var existing = _context.FitUsers.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteFitUser(Guid id)
        {
            var entity = _context.FitUsers.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<FitUser> FindFitUsersByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(FitUser), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(FitUser.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(FitUser).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<FitUser, bool>>(predicate, parameter);
            return _context.FitUsers.Where(lambda).ToList();
        }
    }
}

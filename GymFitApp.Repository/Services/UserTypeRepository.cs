using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class UserTypeRepository : IUserTypeRepository
    {
        private readonly FitGymAppContext _context;
        public UserTypeRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public UserType CreateUserType(UserType entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.UserTypes.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public UserType GetUserTypeById(Guid id)
        {
            return _context.UserTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<UserType> GetAllUserTypes()
        {
            return _context.UserTypes.Where(e => e.IsActive).ToList();
        }

        public bool UpdateUserType(UserType entity)
        {
            var existing = _context.UserTypes.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteUserType(Guid id)
        {
            var entity = _context.UserTypes.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<UserType> FindUserTypesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(UserType), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(UserType.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(UserType).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<UserType, bool>>(predicate, parameter);
            return _context.UserTypes.Where(lambda).ToList();
        }
    }
}

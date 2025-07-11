using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using GymFitApp.Repository.Services.Interfaces;

namespace GymFitApp.Repository.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly GymFitAppContext _context;

        public UserRepository(GymFitAppContext context)
        {
            _context = context;
        }

        public User CreateUser(User user)
        {
            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public User GetUserById(Guid id)
        {
            return _context.Users.FirstOrDefault(u => u.Id == id && u.IsActive);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.Where(u => u.IsActive).ToList();
        }

        public bool UpdateUser(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Id == user.Id && u.IsActive);
            if (existingUser != null)
            {
                _context.Entry(existingUser).CurrentValues.SetValues(user);
                existingUser.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteUser(Guid id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id && u.IsActive);
            if (user != null)
            {
                user.IsActive = false;
                user.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<User> FindUsersByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(User), "u");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(User.IsActive)),
                Expression.Constant(true)
            );

            foreach (var filter in filters)
            {
                var property = typeof(User).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }

            var lambda = Expression.Lambda<Func<User, bool>>(predicate, parameter);
            return _context.Users.Where(lambda).ToList();
        }
    }
}

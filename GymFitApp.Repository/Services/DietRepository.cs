using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class DietRepository : IDietRepository
    {
        private readonly FitGymAppContext _context;
        public DietRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Diet CreateDiet(Diet diet)
        {
            diet.Id = Guid.NewGuid();
            diet.CreatedAt = DateTime.UtcNow;
            diet.IsActive = true;
            _context.Diets.Add(diet);
            _context.SaveChanges();
            return diet;
        }

        public Diet GetDietById(Guid id)
        {
            return _context.Diets.FirstOrDefault(d => d.Id == id && d.IsActive);
        }

        public IEnumerable<Diet> GetAllDiets()
        {
            return _context.Diets.Where(d => d.IsActive).ToList();
        }

        public bool UpdateDiet(Diet diet)
        {
            var existing = _context.Diets.FirstOrDefault(d => d.Id == diet.Id && d.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(diet);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteDiet(Guid id)
        {
            var entity = _context.Diets.FirstOrDefault(d => d.Id == id && d.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Diet> FindDietsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Diet), "d");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Diet.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Diet).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Diet, bool>>(predicate, parameter);
            return _context.Diets.Where(lambda).ToList();
        }
    }
}

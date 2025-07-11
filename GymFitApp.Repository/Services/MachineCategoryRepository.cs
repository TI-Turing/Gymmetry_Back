using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class MachineCategoryRepository : IMachineCategoryRepository
    {
        private readonly FitGymAppContext _context;
        public MachineCategoryRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public MachineCategory CreateMachineCategory(MachineCategory entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _context.MachineCategories.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        public MachineCategory GetMachineCategoryById(Guid id)
        {
            return _context.MachineCategories.FirstOrDefault(e => e.Id == id && e.IsActive);
        }

        public IEnumerable<MachineCategory> GetAllMachineCategories()
        {
            return _context.MachineCategories.Where(e => e.IsActive).ToList();
        }

        public bool UpdateMachineCategory(MachineCategory entity)
        {
            var existing = _context.MachineCategories.FirstOrDefault(e => e.Id == entity.Id && e.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteMachineCategory(Guid id)
        {
            var entity = _context.MachineCategories.FirstOrDefault(e => e.Id == id && e.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<MachineCategory> FindMachineCategoriesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(MachineCategory), "e");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(MachineCategory.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(MachineCategory).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<MachineCategory, bool>>(predicate, parameter);
            return _context.MachineCategories.Where(lambda).ToList();
        }
    }
}

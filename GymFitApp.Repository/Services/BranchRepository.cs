using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class BranchRepository : IBranchRepository
    {
        private readonly FitGymAppContext _context;
        public BranchRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Branch CreateBranch(Branch branch)
        {
            branch.Id = Guid.NewGuid();
            branch.CreatedAt = DateTime.UtcNow;
            branch.IsActive = true;
            _context.Branches.Add(branch);
            _context.SaveChanges();
            return branch;
        }

        public Branch GetBranchById(Guid id)
        {
            return _context.Branches.FirstOrDefault(b => b.Id == id && b.IsActive);
        }

        public IEnumerable<Branch> GetAllBranches()
        {
            return _context.Branches.Where(b => b.IsActive).ToList();
        }

        public bool UpdateBranch(Branch branch)
        {
            var existing = _context.Branches.FirstOrDefault(b => b.Id == branch.Id && b.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(branch);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteBranch(Guid id)
        {
            var entity = _context.Branches.FirstOrDefault(b => b.Id == id && b.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Branch> FindBranchesByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Branch), "b");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Branch.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Branch).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Branch, bool>>(predicate, parameter);
            return _context.Branches.Where(lambda).ToList();
        }
    }
}

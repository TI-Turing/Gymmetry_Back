using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gymmetry.Repository.Services
{
    public class BranchRepository : IBranchRepository
    {
        private readonly GymmetryContext _context;
        
        public BranchRepository(GymmetryContext context)
        {
            _context = context;
        }

        public async Task<Branch> CreateBranchAsync(Branch branch)
        {
            branch.Id = Guid.NewGuid();
            branch.CreatedAt = DateTime.UtcNow;
            branch.IsActive = true;
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return branch;
        }

        public async Task<Branch?> GetBranchByIdAsync(Guid id)
        {
            return await _context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Branch>> GetAllBranchesAsync()
        {
            return await _context.Branches
                .AsNoTracking()
                .Where(b => b.IsActive)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<bool> UpdateBranchAsync(Branch branch)
        {
            var existing = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == branch.Id && b.IsActive)
                .ConfigureAwait(false);
                
            if (existing == null) return false;
            
            _context.Entry(existing).CurrentValues.SetValues(branch);
            _context.Entry(existing).Property(x => x.CreatedAt).IsModified = false;
            existing.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<bool> DeleteBranchAsync(Guid id)
        {
            var entity = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive)
                .ConfigureAwait(false);
                
            if (entity == null) return false;
            
            entity.IsActive = false;
            entity.DeletedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }

        public async Task<IEnumerable<Branch>> FindBranchesByFieldsAsync(Dictionary<string, object> filters)
        {
            var predicate = BuildPredicate(filters);
            return await _context.Branches
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        private static Expression<Func<Branch, bool>> BuildPredicate(Dictionary<string, object> filters)
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
                var right = Expression.Constant(ValueConverter.ConvertValueToType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            
            return Expression.Lambda<Func<Branch, bool>>(predicate, parameter);
        }
    }
}

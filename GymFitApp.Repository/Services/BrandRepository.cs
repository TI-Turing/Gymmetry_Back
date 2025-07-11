using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class BrandRepository : IBrandRepository
    {
        private readonly FitGymAppContext _context;
        public BrandRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Brand CreateBrand(Brand brand)
        {
            brand.Id = Guid.NewGuid();
            brand.CreatedAt = DateTime.UtcNow;
            brand.IsActive = true;
            _context.Brands.Add(brand);
            _context.SaveChanges();
            return brand;
        }

        public Brand GetBrandById(Guid id)
        {
            return _context.Brands.FirstOrDefault(b => b.Id == id && b.IsActive);
        }

        public IEnumerable<Brand> GetAllBrands()
        {
            return _context.Brands.Where(b => b.IsActive).ToList();
        }

        public bool UpdateBrand(Brand brand)
        {
            var existing = _context.Brands.FirstOrDefault(b => b.Id == brand.Id && b.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(brand);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteBrand(Guid id)
        {
            var entity = _context.Brands.FirstOrDefault(b => b.Id == id && b.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Brand> FindBrandsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Brand), "b");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Brand.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Brand).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Brand, bool>>(predicate, parameter);
            return _context.Brands.Where(lambda).ToList();
        }
    }
}

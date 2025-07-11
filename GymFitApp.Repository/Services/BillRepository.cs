using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FitGymApp.Domain.Models;
using FitGymApp.Repository.Services.Interfaces;

namespace FitGymApp.Repository.Services
{
    public class BillRepository : IBillRepository
    {
        private readonly FitGymAppContext _context;
        public BillRepository(FitGymAppContext context)
        {
            _context = context;
        }

        public Bill CreateBill(Bill bill)
        {
            bill.Id = Guid.NewGuid();
            bill.CreatedAt = DateTime.UtcNow;
            bill.IsActive = true;
            _context.Bills.Add(bill);
            _context.SaveChanges();
            return bill;
        }

        public Bill GetBillById(Guid id)
        {
            return _context.Bills.FirstOrDefault(b => b.Id == id && b.IsActive);
        }

        public IEnumerable<Bill> GetAllBills()
        {
            return _context.Bills.Where(b => b.IsActive).ToList();
        }

        public bool UpdateBill(Bill bill)
        {
            var existing = _context.Bills.FirstOrDefault(b => b.Id == bill.Id && b.IsActive);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(bill);
                existing.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public bool DeleteBill(Guid id)
        {
            var entity = _context.Bills.FirstOrDefault(b => b.Id == id && b.IsActive);
            if (entity != null)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public IEnumerable<Bill> FindBillsByFields(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Bill), "b");
            Expression predicate = Expression.Equal(
                Expression.Property(parameter, nameof(Bill.IsActive)),
                Expression.Constant(true)
            );
            foreach (var filter in filters)
            {
                var property = typeof(Bill).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(Convert.ChangeType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<Bill, bool>>(predicate, parameter);
            return _context.Bills.Where(lambda).ToList();
        }
    }
}

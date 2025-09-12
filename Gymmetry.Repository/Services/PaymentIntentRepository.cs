using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Gymmetry.Repository.Services.Interfaces;

namespace Gymmetry.Repository.Services
{
    public class PaymentIntentRepository : IPaymentIntentRepository
    {
        private readonly GymmetryContext _context;
        public PaymentIntentRepository(GymmetryContext context)
        {
            _context = context;
        }

        public async Task<PaymentIntent> AddAsync(PaymentIntent intent)
        {
            intent.Id = intent.Id == Guid.Empty ? Guid.NewGuid() : intent.Id;
            _context.Payments.Add(intent);
            await _context.SaveChangesAsync();
            return intent;
        }

        public Task<PaymentIntent?> GetByPreferenceIdAsync(string preferenceId)
            => _context.Payments.FirstOrDefaultAsync(x => x.PreferenceId == preferenceId && x.IsActive && x.DeletedAt == null);

        public Task<PaymentIntent?> GetByIdAsync(Guid id)
            => _context.Payments.FirstOrDefaultAsync(x => x.Id == id && x.IsActive && x.DeletedAt == null);

        public Task<PaymentIntent?> GetByExternalPaymentIdAsync(string externalId)
            => _context.Payments.FirstOrDefaultAsync(x => x.ExternalPaymentId == externalId && x.IsActive && x.DeletedAt == null);

        public async Task<bool> UpdateAsync(PaymentIntent intent)
        {
            intent.UpdatedAt = DateTime.UtcNow;
            _context.Payments.Update(intent);
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<bool> ExistsPendingForUserPlanAsync(Guid userId, Guid planTypeId)
            => _context.Payments.AnyAsync(p => p.UserId == userId && p.PlanTypeId == planTypeId && p.Status == PaymentStatus.Pending && p.IsActive && p.DeletedAt == null);

        public Task<bool> ExistsPendingForGymPlanAsync(Guid gymId, Guid gymPlanSelectedTypeId)
            => _context.Payments.AnyAsync(p => p.GymId == gymId && p.GymPlanSelectedTypeId == gymPlanSelectedTypeId && p.Status == PaymentStatus.Pending && p.IsActive && p.DeletedAt == null);

        public async Task<IEnumerable<PaymentIntent>> GetAllAsync()
        {
            return await _context.Payments.Where(p => p.IsActive && p.DeletedAt == null).ToListAsync();
        }

        public async Task<IEnumerable<PaymentIntent>> FindByFieldsAsync(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(PaymentIntent), "e");
            Expression predicate = Expression.AndAlso(
                Expression.Equal(Expression.Property(parameter, nameof(PaymentIntent.IsActive)), Expression.Constant(true)),
                Expression.Equal(Expression.Property(parameter, nameof(PaymentIntent.DeletedAt)), Expression.Constant(null, typeof(DateTime?)))
            );
            foreach (var filter in filters)
            {
                var property = typeof(PaymentIntent).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(ValueConverter.ConvertValueToType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }
            var lambda = Expression.Lambda<Func<PaymentIntent, bool>>(predicate, parameter);
            return await _context.Payments.Where(lambda).ToListAsync();
        }
    }
}

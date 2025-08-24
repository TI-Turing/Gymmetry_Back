using System;
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
            => _context.Payments.FirstOrDefaultAsync(x => x.PreferenceId == preferenceId);

        public Task<PaymentIntent?> GetByIdAsync(Guid id)
            => _context.Payments.FirstOrDefaultAsync(x => x.Id == id);

        public Task<PaymentIntent?> GetByExternalPaymentIdAsync(string externalId)
            => _context.Payments.FirstOrDefaultAsync(x => x.ExternalPaymentId == externalId);

        public async Task<bool> UpdateAsync(PaymentIntent intent)
        {
            intent.UpdatedAt = DateTime.UtcNow;
            _context.Payments.Update(intent);
            return await _context.SaveChangesAsync() > 0;
        }

        public Task<bool> ExistsPendingForUserPlanAsync(Guid userId, Guid planTypeId)
            => _context.Payments.AnyAsync(p => p.UserId == userId && p.PlanTypeId == planTypeId && p.Status == PaymentStatus.Pending);

        public Task<bool> ExistsPendingForGymPlanAsync(Guid gymId, Guid gymPlanSelectedTypeId)
            => _context.Payments.AnyAsync(p => p.GymId == gymId && p.GymPlanSelectedTypeId == gymPlanSelectedTypeId && p.Status == PaymentStatus.Pending);
    }
}

using System;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using System.Collections.Generic;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IPaymentIntentRepository
    {
        Task<PaymentIntent> AddAsync(PaymentIntent intent);
        Task<PaymentIntent?> GetByPreferenceIdAsync(string preferenceId);
        Task<PaymentIntent?> GetByIdAsync(Guid id);
        Task<PaymentIntent?> GetByExternalPaymentIdAsync(string externalId);
        Task<bool> UpdateAsync(PaymentIntent intent);
        Task<bool> ExistsPendingForUserPlanAsync(Guid userId, Guid planTypeId);
        Task<bool> ExistsPendingForGymPlanAsync(Guid gymId, Guid gymPlanSelectedTypeId);
        Task<IEnumerable<PaymentIntent>> GetAllAsync();
        Task<IEnumerable<PaymentIntent>> FindByFieldsAsync(Dictionary<string, object> filters);
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Application.Services
{
    public class PaymentIntentService : IPaymentIntentService
    {
        private readonly IPaymentIntentRepository _repo;
        private readonly IPlanRepository _planRepo;
        private readonly IGymPlanSelectedRepository _gymPlanRepo;
        private readonly ILogger<PaymentIntentService> _logger;

        public PaymentIntentService(IPaymentIntentRepository repo, IPlanRepository planRepo, IGymPlanSelectedRepository gymPlanRepo, ILogger<PaymentIntentService> logger)
        {
            _repo = repo;
            _planRepo = planRepo;
            _gymPlanRepo = gymPlanRepo;
            _logger = logger;
        }

        public async Task<PaymentIntent?> GetAsync(string idOrExternal)
        {
            if (Guid.TryParse(idOrExternal, out var guid))
            {
                var byId = await _repo.GetByIdAsync(guid);
                if (byId != null) return byId;
            }
            var byExternal = await _repo.GetByExternalPaymentIdAsync(idOrExternal);
            return byExternal;
        }

        public async Task<string> ProcessExternalPaymentAsync(string externalPaymentId, IPaymentGatewayService gateway)
        {
            var details = await gateway.GetPaymentDetailsAsync(externalPaymentId);
            var intent = details.PreferenceId != null ? await _repo.GetByPreferenceIdAsync(details.PreferenceId) : await _repo.GetByExternalPaymentIdAsync(externalPaymentId);
            if (intent == null) return "no-intent";
            if (intent.Status == PaymentStatus.Approved && intent.CreatedPlanId.HasValue) return "already-processed";
            intent.ExternalPaymentId = externalPaymentId;
            intent.RawPaymentJson = details.RawJson;
            intent.Status = details.Status;
            if (details.Status == PaymentStatus.Approved)
            {
                var start = DateTime.UtcNow;
                var end = start.AddDays(30).AddSeconds(-1);
                if (intent.UserId.HasValue && intent.PlanTypeId.HasValue)
                {
                    var activeUserPlan = await _planRepo.FindPlansByFieldsAsync(new System.Collections.Generic.Dictionary<string, object>{{"UserId", intent.UserId.Value},{"IsActive", true}});
                    if (!activeUserPlan.Any(p => p.EndDate > DateTime.UtcNow))
                    {
                        var plan = new Plan { Id = Guid.NewGuid(), UserId = intent.UserId.Value, PlanTypeId = intent.PlanTypeId.Value, StartDate = start, EndDate = end, CreatedAt = start, UpdatedAt = start, IsActive = true };
                        await _planRepo.CreatePlanAsync(plan);
                        intent.CreatedPlanId = plan.Id;
                    }
                }
                else if (intent.GymId.HasValue && intent.GymPlanSelectedTypeId.HasValue)
                {
                    var activeGymPlans = await _gymPlanRepo.FindGymPlanSelectedsByFieldsAsync(new System.Collections.Generic.Dictionary<string, object>{{"GymId", intent.GymId.Value},{"IsActive", true}});
                    if (!activeGymPlans.Any(p => p.EndDate > DateTime.UtcNow))
                    {
                        var gymPlan = new GymPlanSelected { Id = Guid.NewGuid(), GymId = intent.GymId.Value, GymPlanSelectedTypeId = intent.GymPlanSelectedTypeId.Value, StartDate = start, EndDate = end, CreatedAt = start, UpdatedAt = start, IsActive = true };
                        await _gymPlanRepo.CreateGymPlanSelectedAsync(gymPlan);
                        intent.CreatedPlanId = gymPlan.Id;
                    }
                }
            }
            else if (intent.Status == PaymentStatus.Pending && intent.ExpiresAt.HasValue && intent.ExpiresAt < DateTime.UtcNow && details.Status == PaymentStatus.Approved)
            {
                // Política: ignorar aprobación tardía manteniendo Expired si ya estaba marcado; si aún estaba Pending lo pasamos a Expired
                intent.Status = PaymentStatus.Expired;
                intent.RawPaymentJson = details.RawJson;
                await _repo.UpdateAsync(intent);
                return "expired";
            }
            await _repo.UpdateAsync(intent);
            return "ok";
        }
    }
}

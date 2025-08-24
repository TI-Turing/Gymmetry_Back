using System;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Gymmetry.Functions.Payments
{
    public class PaymentIntentExpirationFunction
    {
        private readonly GymmetryContext _ctx;
        private readonly ILogger<PaymentIntentExpirationFunction> _logger;
        public PaymentIntentExpirationFunction(GymmetryContext ctx, ILogger<PaymentIntentExpirationFunction> logger)
        {
            _ctx = ctx;
            _logger = logger;
        }

        // Ejecuta cada 15 minutos
        [Function("PaymentIntents_ExpirePending")]
        public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo timer)
        {
            var now = DateTime.UtcNow;
            var toExpire = await _ctx.Payments
                .Where(p => p.Status == PaymentStatus.Pending && p.ExpiresAt != null && p.ExpiresAt < now)
                .ToListAsync();
            if (toExpire.Any())
            {
                foreach (var pi in toExpire)
                {
                    pi.Status = PaymentStatus.Expired;
                    pi.UpdatedAt = now;
                }
                await _ctx.SaveChangesAsync();
                _logger.LogInformation("Expired {Count} payment intents", toExpire.Count);
            }
        }
    }
}

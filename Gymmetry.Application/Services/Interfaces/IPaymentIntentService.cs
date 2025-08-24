using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using Gymmetry.Application.Services.Interfaces;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IPaymentIntentService
    {
        Task<PaymentIntent?> GetAsync(string idOrExternal);
        Task<string> ProcessExternalPaymentAsync(string externalPaymentId, IPaymentGatewayService gateway);
    }
}

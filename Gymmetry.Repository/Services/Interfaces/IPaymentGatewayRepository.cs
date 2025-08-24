using System.Threading.Tasks;
using Gymmetry.Domain.DTO.Payments.Responses;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IPaymentGatewayRepository
    {
        Task<PreferenceCreationResult> CreatePreferenceAsync(string title, decimal price, string currency, string? successUrl, string? failureUrl, string? pendingUrl, string notificationUrl);
        Task<PaymentDetailsResult> GetPaymentDetailsAsync(string externalPaymentId);
        Task<PaymentDetailsResult> CreateCardPaymentAsync(decimal amount, string currency, string token, int installments, string description, string? payerEmail, object? metadata);
        bool VerifyWebhookSignature(System.Collections.Generic.IDictionary<string,string> headers, string rawBody);
    }
}

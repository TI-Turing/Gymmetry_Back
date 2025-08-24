using System.Threading.Tasks;
using Gymmetry.Domain.DTO.Payments.Responses;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IPayURepository
    {
        Task<(PreferenceCreationResult Result, string Signature)> CreateTransactionAsync(string reference, string description, decimal amount, string currency, string responseUrl, string confirmationUrl, string buyerEmail, string paymentMethod = "VISA", string? bankCode = null);
        Task<PaymentDetailsResult> GetPaymentDetailsAsync(string referenceCode);
        string ComputeSignature(string raw);
        bool VerifyNotificationSignature(string providedSignature, string reference, decimal amount, string currency, string statePol);
        PaymentStatus MapStatePol(string statePol);
    }
}

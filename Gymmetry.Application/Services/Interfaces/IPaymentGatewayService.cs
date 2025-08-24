using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO.Payments.Requests;
using Gymmetry.Domain.DTO.Payments.Responses;
using Gymmetry.Domain.Models;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IPaymentGatewayService
    {
        Task<PaymentPreferenceResponse> CreateUserPlanPreferenceAsync(CreateUserPlanPreferenceRequest request);
        Task<PaymentPreferenceResponse> CreateGymPlanPreferenceAsync(CreateGymPlanPreferenceRequest request);
        Task<(PaymentStatus Status, string RawJson, decimal? Amount, string? Currency, string? PreferenceId, string? ExternalPaymentId)> GetPaymentDetailsAsync(string externalPaymentId);
        bool VerifyWebhookSignature(IDictionary<string, string> headers, string rawBody);
        Task<PaymentPreferenceResponse> CreateUserPlanCardPaymentAsync(CreateCardPaymentRequest request);
        Task<PaymentPreferenceResponse> CreateGymPlanCardPaymentAsync(CreateCardPaymentRequest request);
    }
}

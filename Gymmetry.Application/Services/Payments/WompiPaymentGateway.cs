using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO.Payments.Requests;
using Gymmetry.Domain.DTO.Payments.Responses;
using Gymmetry.Domain.Models;

namespace Gymmetry.Application.Services.Payments
{
    // Adaptado al nuevo contrato. Actualmente no usado, retorna NotSupported para preferencias.
    public class WompiPaymentGateway : IPaymentGatewayService
    {
        public Task<PaymentPreferenceResponse> CreateGymPlanPreferenceAsync(CreateGymPlanPreferenceRequest request)
            => Task.FromException<PaymentPreferenceResponse>(new NotSupportedException("Wompi no implementado para gym planes aún"));

        public Task<PaymentPreferenceResponse> CreateUserPlanPreferenceAsync(CreateUserPlanPreferenceRequest request)
            => Task.FromException<PaymentPreferenceResponse>(new NotSupportedException("Wompi no implementado para user planes aún"));

        public Task<(PaymentStatus Status, string RawJson, decimal? Amount, string? Currency, string? PreferenceId, string? ExternalPaymentId)> GetPaymentDetailsAsync(string externalPaymentId)
            => Task.FromResult<(PaymentStatus, string, decimal?, string?, string?, string?)>((PaymentStatus.Pending, string.Empty, null, null, null, externalPaymentId));

        public bool VerifyWebhookSignature(System.Collections.Generic.IDictionary<string, string> headers, string rawBody) => true;
        public Task<PaymentPreferenceResponse> CreateUserPlanCardPaymentAsync(CreateCardPaymentRequest request)
            => Task.FromException<PaymentPreferenceResponse>(new NotSupportedException("Wompi card flow no implementado"));
        public Task<PaymentPreferenceResponse> CreateGymPlanCardPaymentAsync(CreateCardPaymentRequest request)
            => Task.FromException<PaymentPreferenceResponse>(new NotSupportedException("Wompi card flow no implementado"));
    }
}

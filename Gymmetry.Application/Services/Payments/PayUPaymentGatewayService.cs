using System;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO.Payments.Requests;
using Gymmetry.Domain.DTO.Payments.Responses;
using Gymmetry.Domain.Models;
using Gymmetry.Domain.Options;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gymmetry.Application.Services.Payments
{
    public class PayUPaymentGatewayService : IPaymentGatewayService
    {
        private readonly ILogger<PayUPaymentGatewayService> _logger;
        private readonly PaymentsOptions _paymentsOptions;
        private readonly IPaymentIntentRepository _intentRepo;
        private readonly IPayURepository _payuRepo;
        private readonly GymmetryContext _ctx;

        public PayUPaymentGatewayService(ILogger<PayUPaymentGatewayService> logger,
            IOptions<PaymentsOptions> paymentsOptions,
            IPaymentIntentRepository intentRepo,
            IPayURepository payuRepo,
            GymmetryContext ctx)
        {
            _logger = logger;
            _paymentsOptions = paymentsOptions.Value;
            _intentRepo = intentRepo;
            _payuRepo = payuRepo;
            _ctx = ctx;
        }

        public async Task<PaymentPreferenceResponse> CreateUserPlanPreferenceAsync(CreateUserPlanPreferenceRequest request)
        {
            var planType = await _ctx.PlanTypes.FindAsync(request.PlanTypeId);
            if (planType == null || !planType.IsActive) throw new System.InvalidOperationException("PlanType no encontrado o inactivo");
            if (planType.Price == null || planType.Price <= 0) throw new System.InvalidOperationException("Plan gratuito - no requiere pago");
            if (await _intentRepo.ExistsPendingForUserPlanAsync(request.UserId, request.PlanTypeId)) throw new System.InvalidOperationException("Ya existe un pago pendiente para este plan del usuario");
            var reference = $"USER-{request.UserId}-{System.Guid.NewGuid():N}";
            var responseUrl = _paymentsOptions.BaseSuccessUrl;
            var confirmUrl = System.Environment.GetEnvironmentVariable("APP_BASE_URL_PAGOS") + "/payments/webhook/payu";
            var paymentMethod = string.Equals(request.PaymentMethod, "PSE", StringComparison.OrdinalIgnoreCase) ? "PSE" : "VISA";
            var bankCode = request.BankCode;
            var buyerEmail = request.BuyerEmail ?? "user@example.com";
            var (pref, signature) = await _payuRepo.CreateTransactionAsync(reference, planType.Name, planType.Price ?? 0, "COP", responseUrl!, confirmUrl!, buyerEmail, paymentMethod, bankCode);
            var intent = new PaymentIntent
            {
                PreferenceId = pref.PreferenceId,
                UserId = request.UserId,
                PlanTypeId = request.PlanTypeId,
                Amount = planType.Price ?? 0,
                Currency = "COP",
                Status = PaymentStatus.Pending,
                RawPreferenceJson = pref.RawJson,
                Hash = signature,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_paymentsOptions.PendingTTLMinutes),
                PaymentMethod = paymentMethod,
                BankCode = bankCode
            };
            await _intentRepo.AddAsync(intent);
            return new PaymentPreferenceResponse { Id = pref.PreferenceId, InitPoint = pref.InitPoint, SandboxInitPoint = pref.SandboxInitPoint, Status = "pending" };
        }

        public async Task<PaymentPreferenceResponse> CreateGymPlanPreferenceAsync(CreateGymPlanPreferenceRequest request)
        {
            var gymPlanType = await _ctx.GymPlanSelectedTypes.FindAsync(request.GymPlanSelectedTypeId);
            if (gymPlanType == null || !gymPlanType.IsActive) throw new System.InvalidOperationException("GymPlanSelectedType no encontrado o inactivo");
            if (gymPlanType.Price == null || gymPlanType.Price <= 0) throw new System.InvalidOperationException("Plan de gimnasio gratuito - no requiere pago");
            if (await _intentRepo.ExistsPendingForGymPlanAsync(request.GymId, request.GymPlanSelectedTypeId)) throw new System.InvalidOperationException("Ya existe un pago pendiente para este plan del gimnasio");
            var reference = $"GYM-{request.GymId}-{System.Guid.NewGuid():N}";
            var responseUrl = _paymentsOptions.BaseSuccessUrl;
            var confirmUrl = System.Environment.GetEnvironmentVariable("APP_BASE_URL_PAGOS") + "/payments/webhook/payu";
            var paymentMethod = string.Equals(request.PaymentMethod, "PSE", StringComparison.OrdinalIgnoreCase) ? "PSE" : "VISA";
            var bankCode = request.BankCode;
            var buyerEmail = request.BuyerEmail ?? "owner@example.com";
            var (pref, signature) = await _payuRepo.CreateTransactionAsync(reference, gymPlanType.Name, gymPlanType.Price ?? 0, "COP", responseUrl!, confirmUrl!, buyerEmail, paymentMethod, bankCode);
            var intent = new PaymentIntent
            {
                PreferenceId = pref.PreferenceId,
                GymId = request.GymId,
                GymPlanSelectedTypeId = request.GymPlanSelectedTypeId,
                Amount = gymPlanType.Price ?? 0,
                Currency = "COP",
                Status = PaymentStatus.Pending,
                RawPreferenceJson = pref.RawJson,
                Hash = signature,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_paymentsOptions.PendingTTLMinutes),
                PaymentMethod = paymentMethod,
                BankCode = bankCode
            };
            await _intentRepo.AddAsync(intent);
            return new PaymentPreferenceResponse { Id = pref.PreferenceId, InitPoint = pref.InitPoint, SandboxInitPoint = pref.SandboxInitPoint, Status = "pending" };
        }

        public async Task<(PaymentStatus Status, string RawJson, decimal? Amount, string? Currency, string? PreferenceId, string? ExternalPaymentId)> GetPaymentDetailsAsync(string externalPaymentId)
        {
            var details = await _payuRepo.GetPaymentDetailsAsync(externalPaymentId);
            return (details.Status, details.RawJson, details.Amount, details.Currency, details.PreferenceId, details.ExternalPaymentId);
        }

        public bool VerifyWebhookSignature(System.Collections.Generic.IDictionary<string, string> headers, string rawBody) => true; // TODO: implementar validación de firma PayU
        public Task<PaymentPreferenceResponse> CreateUserPlanCardPaymentAsync(CreateCardPaymentRequest request)
            => Task.FromException<PaymentPreferenceResponse>(new NotSupportedException("PayU card flow no implementado"));
        public Task<PaymentPreferenceResponse> CreateGymPlanCardPaymentAsync(CreateCardPaymentRequest request)
            => Task.FromException<PaymentPreferenceResponse>(new NotSupportedException("PayU card flow no implementado"));
    }
}

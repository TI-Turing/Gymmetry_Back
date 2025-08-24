using System;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO.Payments.Requests;
using Gymmetry.Domain.DTO.Payments.Responses;
using Gymmetry.Domain.Models;
using Gymmetry.Domain.Options;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gymmetry.Application.Services.Payments
{
    public class MercadoPagoPaymentGatewayService : IPaymentGatewayService
    {
        private readonly ILogger<MercadoPagoPaymentGatewayService> _logger;
        private readonly PaymentsOptions _options;
        private readonly IPaymentIntentRepository _paymentIntentRepository;
        private readonly GymmetryContext _context;
        private readonly IPaymentGatewayRepository _gatewayRepo;

        public MercadoPagoPaymentGatewayService(
            ILogger<MercadoPagoPaymentGatewayService> logger,
            IOptions<PaymentsOptions> options,
            IPaymentIntentRepository paymentIntentRepository,
            GymmetryContext context,
            IPaymentGatewayRepository gatewayRepo)
        {
            _logger = logger;
            _options = options.Value;
            _paymentIntentRepository = paymentIntentRepository;
            _context = context;
            _gatewayRepo = gatewayRepo;
        }

        public async Task<PaymentPreferenceResponse> CreateUserPlanPreferenceAsync(CreateUserPlanPreferenceRequest request)
        {
            var planType = await _context.PlanTypes.FindAsync(request.PlanTypeId);
            if (planType == null || !planType.IsActive)
                throw new InvalidOperationException("PlanType no encontrado o inactivo");
            if (planType.Price == null || planType.Price <= 0)
                throw new InvalidOperationException("Plan gratuito - no requiere pago");
            if (await _paymentIntentRepository.ExistsPendingForUserPlanAsync(request.UserId, request.PlanTypeId))
                throw new InvalidOperationException("Ya existe un pago pendiente para este plan del usuario");

            var notificationUrl = Environment.GetEnvironmentVariable("APP_BASE_URL_PAGOS") + "/payments/webhook/mercadopago";
            var pref = await _gatewayRepo.CreatePreferenceAsync(planType.Name, planType.Price ?? 0, "COP", request.SuccessUrl ?? _options.BaseSuccessUrl, request.FailureUrl ?? _options.BaseFailureUrl, request.PendingUrl ?? _options.BasePendingUrl, notificationUrl);
            var intent = new PaymentIntent
            {
                PreferenceId = pref.PreferenceId,
                UserId = request.UserId,
                PlanTypeId = request.PlanTypeId,
                Amount = planType.Price ?? 0,
                Currency = "COP",
                Status = PaymentStatus.Pending,
                RawPreferenceJson = pref.RawJson,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.PendingTTLMinutes),
                PaymentMethod = request.PaymentMethod ?? "CARD",
                BankCode = request.BankCode
            };
            await _paymentIntentRepository.AddAsync(intent);
            _logger.LogInformation("Preference creada {PreferenceId} para User {UserId}", pref.PreferenceId, request.UserId);
            return new PaymentPreferenceResponse { Id = pref.PreferenceId, InitPoint = pref.InitPoint, SandboxInitPoint = pref.SandboxInitPoint, Status = "pending" };
        }

        public async Task<PaymentPreferenceResponse> CreateGymPlanPreferenceAsync(CreateGymPlanPreferenceRequest request)
        {
            var gymPlanType = await _context.GymPlanSelectedTypes.FindAsync(request.GymPlanSelectedTypeId);
            if (gymPlanType == null || !gymPlanType.IsActive)
                throw new InvalidOperationException("GymPlanSelectedType no encontrado o inactivo");
            if (gymPlanType.Price == null || gymPlanType.Price <= 0)
                throw new InvalidOperationException("Plan de gimnasio gratuito - no requiere pago");
            if (await _paymentIntentRepository.ExistsPendingForGymPlanAsync(request.GymId, request.GymPlanSelectedTypeId))
                throw new InvalidOperationException("Ya existe un pago pendiente para este plan del gimnasio");

            var notificationUrl = Environment.GetEnvironmentVariable("APP_BASE_URL_PAGOS") + "/payments/webhook/mercadopago";
            var pref = await _gatewayRepo.CreatePreferenceAsync(gymPlanType.Name, gymPlanType.Price ?? 0, "COP", request.SuccessUrl ?? _options.BaseSuccessUrl, request.FailureUrl ?? _options.BaseFailureUrl, request.PendingUrl ?? _options.BasePendingUrl, notificationUrl);
            var intent = new PaymentIntent
            {
                PreferenceId = pref.PreferenceId,
                GymId = request.GymId,
                GymPlanSelectedTypeId = request.GymPlanSelectedTypeId,
                Amount = gymPlanType.Price ?? 0,
                Currency = "COP",
                Status = PaymentStatus.Pending,
                RawPreferenceJson = pref.RawJson,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.PendingTTLMinutes),
                PaymentMethod = request.PaymentMethod ?? "CARD",
                BankCode = request.BankCode
            };
            await _paymentIntentRepository.AddAsync(intent);
            _logger.LogInformation("Preference creada {PreferenceId} para Gym {GymId}", pref.PreferenceId, request.GymId);
            return new PaymentPreferenceResponse { Id = pref.PreferenceId, InitPoint = pref.InitPoint, SandboxInitPoint = pref.SandboxInitPoint, Status = "pending" };
        }

        public async Task<PaymentPreferenceResponse> CreateUserPlanCardPaymentAsync(CreateCardPaymentRequest request)
        {
            // Validar plan precio y duplicados
            var planType = await _context.PlanTypes.FindAsync(request.PlanTypeId);
            if (planType == null || !planType.IsActive) throw new InvalidOperationException("PlanType no encontrado o inactivo");
            if (planType.Price == null || planType.Price <= 0) throw new InvalidOperationException("Plan gratuito - no requiere pago");
            if (await _paymentIntentRepository.ExistsPendingForUserPlanAsync(request.UserId, request.PlanTypeId!.Value))
            {
                var existing = await _context.Payments.FirstOrDefaultAsync(p => p.UserId == request.UserId && p.PlanTypeId == request.PlanTypeId && p.Status == PaymentStatus.Pending);
                return new PaymentPreferenceResponse { Id = existing!.PreferenceId, InitPoint = string.Empty, Status = "pending" };
            }
            var amount = planType.Price ?? 0;
            var details = await _gatewayRepo.CreateCardPaymentAsync(amount, "COP", request.CardToken, request.Installments, $"Compra de plan {planType.Id}", request.BuyerEmail, new { type = "user", request.UserId, request.PlanTypeId });
            var intent = new PaymentIntent
            {
                PreferenceId = details.ExternalPaymentId ?? Guid.NewGuid().ToString("N"),
                ExternalPaymentId = details.ExternalPaymentId,
                UserId = request.UserId,
                PlanTypeId = request.PlanTypeId,
                Amount = amount,
                Currency = "COP",
                Status = details.Status,
                RawPaymentJson = details.RawJson,
                PaymentMethod = "CARD",
                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.PendingTTLMinutes)
            };
            await _paymentIntentRepository.AddAsync(intent);
            return new PaymentPreferenceResponse { Id = intent.PreferenceId, InitPoint = string.Empty, Status = details.Status.ToString().ToLowerInvariant() };
        }

        public async Task<PaymentPreferenceResponse> CreateGymPlanCardPaymentAsync(CreateCardPaymentRequest request)
        {
            var gymPlanType = await _context.GymPlanSelectedTypes.FindAsync(request.GymPlanSelectedTypeId);
            if (gymPlanType == null || !gymPlanType.IsActive) throw new InvalidOperationException("GymPlanSelectedType no encontrado o inactivo");
            if (gymPlanType.Price == null || gymPlanType.Price <= 0) throw new InvalidOperationException("Plan de gimnasio gratuito - no requiere pago");
            if (await _paymentIntentRepository.ExistsPendingForGymPlanAsync(request.GymId!.Value, request.GymPlanSelectedTypeId!.Value))
            {
                var existing = await _context.Payments.FirstOrDefaultAsync(p => p.GymId == request.GymId && p.GymPlanSelectedTypeId == request.GymPlanSelectedTypeId && p.Status == PaymentStatus.Pending);
                return new PaymentPreferenceResponse { Id = existing!.PreferenceId, InitPoint = string.Empty, Status = "pending" };
            }
            var amount = gymPlanType.Price ?? 0;
            var details = await _gatewayRepo.CreateCardPaymentAsync(amount, "COP", request.CardToken, request.Installments, $"Compra de plan gym {gymPlanType.Id}", request.BuyerEmail, new { type = "gym", request.GymId, request.GymPlanSelectedTypeId });
            var intent = new PaymentIntent
            {
                PreferenceId = details.ExternalPaymentId ?? Guid.NewGuid().ToString("N"),
                ExternalPaymentId = details.ExternalPaymentId,
                GymId = request.GymId,
                GymPlanSelectedTypeId = request.GymPlanSelectedTypeId,
                Amount = amount,
                Currency = "COP",
                Status = details.Status,
                RawPaymentJson = details.RawJson,
                PaymentMethod = "CARD",
                ExpiresAt = DateTime.UtcNow.AddMinutes(_options.PendingTTLMinutes)
            };
            await _paymentIntentRepository.AddAsync(intent);
            return new PaymentPreferenceResponse { Id = intent.PreferenceId, InitPoint = string.Empty, Status = details.Status.ToString().ToLowerInvariant() };
        }

        public async Task<(PaymentStatus Status, string RawJson, decimal? Amount, string? Currency, string? PreferenceId, string? ExternalPaymentId)> GetPaymentDetailsAsync(string externalPaymentId)
        {
            var details = await _gatewayRepo.GetPaymentDetailsAsync(externalPaymentId);
            return (details.Status, details.RawJson, details.Amount, details.Currency, details.PreferenceId, details.ExternalPaymentId);
        }

        public bool VerifyWebhookSignature(IDictionary<string, string> headers, string rawBody) => _gatewayRepo.VerifyWebhookSignature(headers, rawBody);
    }
}

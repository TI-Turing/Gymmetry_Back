using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO.Payments.Responses;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Repository.Services
{
    public class MercadoPagoGatewayRepository : IPaymentGatewayRepository
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MercadoPagoGatewayRepository> _logger;
        private readonly string _accessToken;

        public MercadoPagoGatewayRepository(HttpClient httpClient, ILogger<MercadoPagoGatewayRepository> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _accessToken = Environment.GetEnvironmentVariable("MP_ACCESS_TOKEN") ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(_accessToken))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<PreferenceCreationResult> CreatePreferenceAsync(string title, decimal price, string currency, string? successUrl, string? failureUrl, string? pendingUrl, string notificationUrl)
        {
            var preferencePayload = new
            {
                items = new[] { new { title, quantity = 1, unit_price = price, currency_id = currency } },
                back_urls = new { success = successUrl, failure = failureUrl, pending = pendingUrl },
                auto_return = "approved",
                notification_url = notificationUrl
            };
            var json = JsonSerializer.Serialize(preferencePayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _httpClient.PostAsync("https://api.mercadopago.com/checkout/preferences", content);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Error creando preferencia MP: {Status} {Body}", resp.StatusCode, body);
                throw new InvalidOperationException("No se pudo crear la preferencia de pago");
            }
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            return new PreferenceCreationResult
            {
                PreferenceId = root.GetProperty("id").GetString()!,
                InitPoint = root.GetProperty("init_point").GetString()!,
                SandboxInitPoint = root.TryGetProperty("sandbox_init_point", out var sbox) ? sbox.GetString() : null,
                RawJson = body
            };
        }

        public async Task<PaymentDetailsResult> CreateCardPaymentAsync(decimal amount, string currency, string token, int installments, string description, string? payerEmail, object? metadata)
        {
            var payload = new
            {
                transaction_amount = amount,
                token,
                installments,
                description,
                payment_method_id = "visa", // Bricks deduce por token; este campo puede omitirse
                payer = new { email = payerEmail },
                metadata
            };
            var json = JsonSerializer.Serialize(payload);
            var resp = await _httpClient.PostAsync("https://api.mercadopago.com/v1/payments", new StringContent(json, Encoding.UTF8, "application/json"));
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Error creando pago tarjeta MP: {Status} {Body}", resp.StatusCode, body);
                // Devolver Pending con RawJson para que el application layer decida el mensaje
                return new PaymentDetailsResult { Status = PaymentStatus.Rejected, RawJson = body };
            }
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var statusStr = root.TryGetProperty("status", out var st) ? st.GetString() : null;
            var id = root.TryGetProperty("id", out var idEl) ? idEl.ToString() : null;
            var mapped = statusStr switch
            {
                "approved" => PaymentStatus.Approved,
                "rejected" => PaymentStatus.Rejected,
                "cancelled" => PaymentStatus.Cancelled,
                "cancelled_by_user" => PaymentStatus.Cancelled,
                "pending" => PaymentStatus.Pending,
                _ => PaymentStatus.Pending
            };
            return new PaymentDetailsResult { Status = mapped, RawJson = body, ExternalPaymentId = id, Currency = currency, Amount = amount };
        }

        public async Task<PaymentDetailsResult> GetPaymentDetailsAsync(string externalPaymentId)
        {
            var resp = await _httpClient.GetAsync($"https://api.mercadopago.com/v1/payments/{externalPaymentId}");
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("No se pudo obtener pago externo {ExternalPaymentId}: {Status}", externalPaymentId, resp.StatusCode);
                return new PaymentDetailsResult { Status = PaymentStatus.Pending, RawJson = body, ExternalPaymentId = externalPaymentId };
            }
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            var statusStr = root.TryGetProperty("status", out var st) ? st.GetString() : null;
            var preferenceId = root.TryGetProperty("order", out var orderEl) && orderEl.TryGetProperty("id", out var pref) ? pref.GetString() : null;
            var transactionAmount = root.TryGetProperty("transaction_amount", out var amt) ? amt.GetDecimal() : (decimal?)null;
            var currency = root.TryGetProperty("currency_id", out var cur) ? cur.GetString() : null;
            var mapped = statusStr switch
            {
                "approved" => PaymentStatus.Approved,
                "rejected" => PaymentStatus.Rejected,
                "cancelled" => PaymentStatus.Cancelled,
                "cancelled_by_user" => PaymentStatus.Cancelled,
                "expired" => PaymentStatus.Expired,
                _ => PaymentStatus.Pending
            };
            return new PaymentDetailsResult
            {
                Status = mapped,
                RawJson = body,
                Amount = transactionAmount,
                Currency = currency,
                PreferenceId = preferenceId,
                ExternalPaymentId = externalPaymentId
            };
        }

        public bool VerifyWebhookSignature(System.Collections.Generic.IDictionary<string, string> headers, string rawBody) => true;
    }
}

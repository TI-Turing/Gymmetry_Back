using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO.Payments.Responses;
using Gymmetry.Domain.Models;
using Gymmetry.Domain.Options;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gymmetry.Repository.Services
{
    public class PayURepository : IPayURepository
    {
        private readonly HttpClient _http;
        private readonly ILogger<PayURepository> _logger;
        private readonly PayUOptions _opts;

        public PayURepository(HttpClient http, ILogger<PayURepository> logger, IOptions<PayUOptions> opts)
        {
            _http = http;
            _logger = logger;
            _opts = opts.Value;
        }

        public async Task<(PreferenceCreationResult Result, string Signature)> CreateTransactionAsync(string reference, string description, decimal amount, string currency, string responseUrl, string confirmationUrl, string buyerEmail, string paymentMethod = "VISA", string? bankCode = null)
        {
            var amountStr = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            var sigRaw = string.Join("~", _opts.ApiKey, _opts.MerchantId, reference, amountStr, currency);
            var signature = ComputeSignature(sigRaw);
            var isPse = string.Equals(paymentMethod, "PSE", StringComparison.OrdinalIgnoreCase);
            var payload = new
            {
                language = "es",
                command = "SUBMIT_TRANSACTION",
                merchant = new { apiKey = _opts.ApiKey, apiLogin = _opts.ApiLogin },
                transaction = new
                {
                    order = new
                    {
                        accountId = _opts.AccountId,
                        referenceCode = reference,
                        description,
                        language = "es",
                        signature,
                        notifyUrl = confirmationUrl,
                        additionalValues = new { TX_VALUE = new { value = amount, currency } },
                        buyer = new { emailAddress = buyerEmail }
                    },
                    payer = new { emailAddress = buyerEmail },
                    type = "AUTHORIZATION_AND_CAPTURE",
                    paymentMethod = isPse ? "PSE" : "VISA",
                    paymentCountry = "CO",
                    extraParameters = isPse ? new { BANK_CODE = bankCode, RESPONSE_URL = responseUrl } : null,
                    deviceSessionId = Guid.NewGuid().ToString("N"),
                    ipAddress = "127.0.0.1",
                    userAgent = "Gymmetry"
                },
                test = true
            };
            var json = JsonSerializer.Serialize(payload);
            var resp = await _http.PostAsync(_opts.BaseUrl, new StringContent(json, Encoding.UTF8, "application/json"));
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogError("Error PayU SUBMIT_TRANSACTION: {Status} {Body}", resp.StatusCode, body);
                throw new InvalidOperationException("No se pudo crear la transacción PayU");
            }
            var pref = new PreferenceCreationResult
            {
                PreferenceId = reference,
                InitPoint = _opts.CheckoutUrl,
                SandboxInitPoint = _opts.CheckoutUrl,
                RawJson = body
            };
            return (pref, signature);
        }

        public async Task<PaymentDetailsResult> GetPaymentDetailsAsync(string referenceCode)
        {
            // TODO: Implementar consulta a REPORTS API
            return new PaymentDetailsResult { Status = PaymentStatus.Pending, PreferenceId = referenceCode };
        }

        public string ComputeSignature(string raw)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(raw));
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public bool VerifyNotificationSignature(string providedSignature, string reference, decimal amount, string currency, string statePol)
        {
            // Notification signature spec: MD5(ApiKey~merchantId~referenceCode~amount~currency~statePol)
            var amountStr = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            var raw = string.Join("~", _opts.ApiKey, _opts.MerchantId, reference, amountStr, currency, statePol);
            var expected = ComputeSignature(raw);
            return string.Equals(providedSignature, expected, StringComparison.OrdinalIgnoreCase);
        }

        public PaymentStatus MapStatePol(string statePol) => statePol switch
        {
            "4" => PaymentStatus.Approved,      // approved
            "6" => PaymentStatus.Rejected,      // rejected
            "104" => PaymentStatus.Rejected,
            "5" => PaymentStatus.Expired,       // expired / expired authorization
            "7" => PaymentStatus.Cancelled,     // pending, treat as pending until update
            _ => PaymentStatus.Pending
        };
    }
}

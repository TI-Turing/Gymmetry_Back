using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.User.Request;
using Gymmetry.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Gymmetry.Functions.UserFunctions
{
    // Obsoleto: mantenido temporalmente para compatibilidad mínima. Generación URL directa removida en nuevo flujo.
    public class PaymentUserPlanFunction
    {
        private readonly ILogger<PaymentUserPlanFunction> _logger;
        public PaymentUserPlanFunction(ILogger<PaymentUserPlanFunction> logger)
        {
            _logger = logger;
        }

        [Function("GeneratePaymentUrl")]
        public async Task<HttpResponseData> DeprecatedGeneratePaymentUrlAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/payment-url")] HttpRequestData req)
        {
            var resp = req.CreateResponse(HttpStatusCode.Gone);
            await resp.WriteAsJsonAsync(new ApiResponse<string>{Success=false, Message="Endpoint obsoleto. Usar /payments/plan/preference", StatusCode=StatusCodes.Status410Gone});
            return resp;
        }

        [Function("PaymentWebhook")]
        public async Task<HttpResponseData> DeprecatedWebhookAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "webhook/payment")] HttpRequestData req)
        {
            var resp = req.CreateResponse(HttpStatusCode.Gone);
            await resp.WriteAsJsonAsync(new ApiResponse<string>{Success=false, Message="Webhook obsoleto. Usar /payments/webhook/mercadopago", StatusCode=StatusCodes.Status410Gone});
            return resp;
        }
    }
}
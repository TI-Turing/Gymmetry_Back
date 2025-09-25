using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Payments.Requests;
using Gymmetry.Domain.DTO.Payments.Responses;
using Gymmetry.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Gymmetry.Utils;
using System.Linq;

namespace Gymmetry.Functions.Payments
{
    public class PaymentsFunctions
    {
        private readonly ILogger<PaymentsFunctions> _logger;
        private readonly IPaymentGatewayService _gateway;
        private readonly IPaymentIntentService _paymentIntentService;
        private readonly IPlanService _planService;
        private readonly IPlanTypeService _planTypeService;
        private readonly IGymPlanSelectedService _gymPlanSelectedService;
        private readonly IGymPlanSelectedTypeService _gymPlanSelectedTypeService;
        private readonly IGymService _gymService;

        public PaymentsFunctions(ILogger<PaymentsFunctions> logger,
            IPaymentGatewayService gateway,
            IPaymentIntentService paymentIntentService,
            IPlanService planService,
            IPlanTypeService planTypeService,
            IGymPlanSelectedService gymPlanSelectedService,
            IGymPlanSelectedTypeService gymPlanSelectedTypeService,
            IGymService gymService)
        {
            _logger = logger;
            _gateway = gateway;
            _paymentIntentService = paymentIntentService;
            _planService = planService;
            _planTypeService = planTypeService;
            _gymPlanSelectedService = gymPlanSelectedService;
            _gymPlanSelectedTypeService = gymPlanSelectedTypeService;
            _gymService = gymService;
        }

        [Function("Payments_CreateUserPlanPreference")]
        public async Task<HttpResponseData> CreateUserPlanPreferenceAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/plan/preference")] HttpRequestData req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message=error, StatusCode=StatusCodes.Status401Unauthorized});
                return unauthorized;
            }
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<CreateUserPlanPreferenceRequest>(body);
            if (request == null || request.UserId != userId)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message="Solicitud inválida", StatusCode=StatusCodes.Status400BadRequest});
                return bad;
            }
            var planTypeResp = await _planTypeService.GetPlanTypeByIdAsync(request.PlanTypeId);
            if (!planTypeResp.Success || planTypeResp.Data == null || !planTypeResp.Data.IsActive)
            {
                var bad2 = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad2.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message="PlanType inactivo o inexistente", StatusCode=StatusCodes.Status400BadRequest});
                return bad2;
            }
            var planType = planTypeResp.Data;
            if (planType.Price == null || planType.Price <= 0)
            {
                var now = DateTime.UtcNow;
                var addRequest = new Domain.DTO.Plan.Request.AddPlanRequest { StartDate = now, EndDate = now.AddDays(30).AddSeconds(-1), PlanTypeId = planType.Id, UserId = request.UserId };
                var created = await _planService.CreatePlanAsync(addRequest);
                var okFree = req.CreateResponse(HttpStatusCode.OK);
                await okFree.WriteAsJsonAsync(new ApiResponse<object>{Success=created.Success, Message="Plan gratuito creado", StatusCode=StatusCodes.Status200OK});
                return okFree;
            }
            try
            {
                var pref = await _gateway.CreateUserPlanPreferenceAsync(request);
                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(new ApiResponse<PaymentPreferenceResponse>{Success=true, Data=pref, StatusCode=StatusCodes.Status200OK, Message="Preferencia creada"});
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando preferencia usuario");
                var err = req.CreateResponse(HttpStatusCode.BadRequest);
                await err.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message=ex.Message, StatusCode=StatusCodes.Status400BadRequest});
                return err;
            }
        }

        [Function("Payments_CreateGymPlanPreference")]
        public async Task<HttpResponseData> CreateGymPlanPreferenceAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/gymplan/preference")] HttpRequestData req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message=error, StatusCode=StatusCodes.Status401Unauthorized});
                return unauthorized;
            }
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<CreateGymPlanPreferenceRequest>(body);
            if (request == null || request.UserId != userId)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message="Solicitud inválida", StatusCode=StatusCodes.Status400BadRequest});
                return bad;
            }
            var gymResp = await _gymService.GetGymByIdAsync(request.GymId);
            if (!gymResp.Success || gymResp.Data == null || gymResp.Data.Owner_UserId != userId)
            {
                var forb = req.CreateResponse(HttpStatusCode.Forbidden);
                await forb.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message="No autorizado para este gimnasio", StatusCode=StatusCodes.Status403Forbidden});
                return forb;
            }
            var gymPlanTypeResp = await _gymPlanSelectedTypeService.GetGymPlanSelectedTypeByIdAsync(request.GymPlanSelectedTypeId);
            if (!gymPlanTypeResp.Success || gymPlanTypeResp.Data == null || !gymPlanTypeResp.Data.IsActive)
            {
                var bad2 = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad2.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message="GymPlanSelectedType inactivo o inexistente", StatusCode=StatusCodes.Status400BadRequest});
                return bad2;
            }
            var gymPlanType = gymPlanTypeResp.Data;
            if (gymPlanType.Price == null || gymPlanType.Price <= 0)
            {
                var now = DateTime.UtcNow;
                var addRequest = new Domain.DTO.GymPlanSelected.Request.AddGymPlanSelectedRequest { StartDate = now, EndDate = now.AddDays(30).AddSeconds(-1), GymPlanSelectedTypeId = gymPlanType.Id, GymId = request.GymId };
                var created = await _gymPlanSelectedService.CreateGymPlanSelectedAsync(addRequest);
                var okFree = req.CreateResponse(HttpStatusCode.OK);
                await okFree.WriteAsJsonAsync(new ApiResponse<object>{Success=created.Success, Message="Plan de gimnasio gratuito creado", StatusCode=StatusCodes.Status200OK});
                return okFree;
            }
            try
            {
                var pref = await _gateway.CreateGymPlanPreferenceAsync(request);
                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(new ApiResponse<PaymentPreferenceResponse>{Success=true, Data=pref, StatusCode=StatusCodes.Status200OK, Message="Preferencia creada"});
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando preferencia gym");
                var err = req.CreateResponse(HttpStatusCode.BadRequest);
                await err.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message=ex.Message, StatusCode=StatusCodes.Status400BadRequest});
                return err;
            }
        }

        [Function("Payments_GetStatus")]
        public async Task<HttpResponseData> GetStatusAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "payments/status/{id}")] HttpRequestData req, string id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message=error, StatusCode=StatusCodes.Status401Unauthorized});
                return unauthorized;
            }
            var intent = await _paymentIntentService.GetAsync(id);
            if (intent == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message="Pago no encontrado", StatusCode=StatusCodes.Status404NotFound});
                return notFound;
            }
            var statusResp = new PaymentStatusResponse
            {
                PaymentId = intent.Id,
                PreferenceId = intent.PreferenceId,
                Status = intent.Status,
                PlanCreated = intent.CreatedPlanId.HasValue,
                CreatedPlanId = intent.CreatedPlanId,
                Type = intent.UserId.HasValue ? "user" : "gym",
                PaymentMethod = intent.PaymentMethod,
                BankCode = intent.BankCode,
                ExpiresAt = intent.ExpiresAt,
                Amount = intent.Amount,
                Currency = intent.Currency
            };
            var okResp = req.CreateResponse(HttpStatusCode.OK);
            await okResp.WriteAsJsonAsync(new ApiResponse<PaymentStatusResponse>{Success=true, Data=statusResp, StatusCode=StatusCodes.Status200OK});
            return okResp;
        }

        [Function("Payments_Webhook_MercadoPago")]
        public async Task<HttpResponseData> WebhookAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/webhook/mercadopago")] HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var paymentId = query.Get("data.id") ?? query.Get("id");
            if (string.IsNullOrEmpty(paymentId))
            {
                var okEmpty = req.CreateResponse(HttpStatusCode.OK);
                await okEmpty.WriteStringAsync("ignored");
                return okEmpty;
            }
            try
            {
                var processed = await _paymentIntentService.ProcessExternalPaymentAsync(paymentId, _gateway);
                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteStringAsync(processed);
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando webhook MP");
                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteStringAsync("error");
                return resp;
            }
        }

        [Function("Payments_Webhook_PayU")]
        public async Task<HttpResponseData> PayUWebhookAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/webhook/payu")] HttpRequestData req)
        {
            // PayU sends form-urlencoded or JSON depending; handle both
            string rawBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dict = System.Web.HttpUtility.ParseQueryString(rawBody);
            string reference = dict["reference_sale"] ?? string.Empty;
            string statePol = dict["state_pol"] ?? string.Empty;
            string signature = dict["sign"] ?? string.Empty;
            decimal amount = 0m;
            decimal.TryParse(dict["value"], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out amount);
            string currency = dict["currency"] ?? "COP";

            if (string.IsNullOrEmpty(reference))
            {
                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync("ignored");
                return ok;
            }
            try
            {
                // We reuse ProcessExternalPaymentAsync but need a mapping; provide reference as externalPaymentId surrogate
                var result = await _paymentIntentService.ProcessExternalPaymentAsync(reference, _gateway);
                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteStringAsync(result);
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error webhook PayU");
                var resp = req.CreateResponse(HttpStatusCode.OK);
                await resp.WriteStringAsync("error");
                return resp;
            }
        }

        [Function("Payments_CreateUserPlanCard")] 
        public async Task<HttpResponseData> CreateUserPlanCardAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/plan/card")] HttpRequestData req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message=error, StatusCode=StatusCodes.Status401Unauthorized});
                return unauthorized;
            }
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<CreateCardPaymentRequest>(body);
            if (request == null || request.UserId != userId || request.PlanTypeId == null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message="Solicitud inválida", StatusCode=StatusCodes.Status400BadRequest});
                return bad;
            }
            try
            {
                var result = await _gateway.CreateUserPlanCardPaymentAsync(request);
                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(new ApiResponse<PaymentPreferenceResponse>{Success=true, Data=result, StatusCode=StatusCodes.Status200OK, Message="Pago creado"});
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando pago tarjeta usuario");
                var err = req.CreateResponse(HttpStatusCode.BadRequest);
                await err.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message=ex.Message, StatusCode=StatusCodes.Status400BadRequest});
                return err;
            }
        }

        [Function("Payments_CreateGymPlanCard")] 
        public async Task<HttpResponseData> CreateGymPlanCardAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/gymplan/card")] HttpRequestData req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message=error, StatusCode=StatusCodes.Status401Unauthorized});
                return unauthorized;
            }
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<CreateCardPaymentRequest>(body);
            if (request == null || request.UserId != userId || request.GymId == null || request.GymPlanSelectedTypeId == null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message="Solicitud inválida", StatusCode=StatusCodes.Status400BadRequest});
                return bad;
            }
            // Autorizar que userId sea owner del gym
            var gymResp = await _gymService.GetGymByIdAsync(request.GymId.Value);
            if (!gymResp.Success || gymResp.Data == null || gymResp.Data.Owner_UserId != userId)
            {
                var forb = req.CreateResponse(HttpStatusCode.Forbidden);
                await forb.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message="No autorizado para este gimnasio", StatusCode=StatusCodes.Status403Forbidden});
                return forb;
            }
            try
            {
                var result = await _gateway.CreateGymPlanCardPaymentAsync(request);
                var ok = req.CreateResponse(HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(new ApiResponse<PaymentPreferenceResponse>{Success=true, Data=result, StatusCode=StatusCodes.Status200OK, Message="Pago creado"});
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando pago tarjeta gym");
                var err = req.CreateResponse(HttpStatusCode.BadRequest);
                await err.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message=ex.Message, StatusCode=StatusCodes.Status400BadRequest});
                return err;
            }
        }
    }
}

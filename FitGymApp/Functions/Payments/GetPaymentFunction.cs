using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;
using Gymmetry.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Functions.Payments
{
    public class GetPaymentFunction
    {
        private readonly ILogger<GetPaymentFunction> _logger;
        private readonly IPaymentService _service;

        public GetPaymentFunction(ILogger<GetPaymentFunction> logger, IPaymentService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("Payments_GetById")] 
        public async Task<HttpResponseData> GetByIdAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "payments/{id:guid}")] HttpRequestData req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<PaymentIntent>{Success=false, Message=error, StatusCode=StatusCodes.Status401Unauthorized});
                return unauthorized;
            }
            var result = await _service.GetPaymentByIdAsync(id);
            var status = result.Success ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            var resp = req.CreateResponse(status);
            await resp.WriteAsJsonAsync(new ApiResponse<PaymentIntent>{Success=result.Success, Message=result.Message, Data=result.Data, StatusCode=(int)status});
            return resp;
        }

        [Function("Payments_GetAll")] 
        public async Task<HttpResponseData> GetAllAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "payments")] HttpRequestData req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<IEnumerable<PaymentIntent>>{Success=false, Message=error, StatusCode=StatusCodes.Status401Unauthorized});
                return unauthorized;
            }
            var result = await _service.GetAllPaymentsAsync();
            var resp = req.CreateResponse(HttpStatusCode.OK);
            await resp.WriteAsJsonAsync(new ApiResponse<IEnumerable<PaymentIntent>>{Success=true, Data=result.Data, StatusCode=StatusCodes.Status200OK});
            return resp;
        }

        [Function("Payments_FindPaymentsByFields")] 
        public async Task<HttpResponseData> FindPaymentsByFieldsAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payments/find")] HttpRequestData req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<IEnumerable<PaymentIntent>>{Success=false, Message=error, StatusCode=StatusCodes.Status401Unauthorized});
                return unauthorized;
            }
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            var filters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(body);
            if (filters == null || filters.Count == 0)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new ApiResponse<IEnumerable<PaymentIntent>>{Success=false, Message="No se proporcionaron filtros válidos.", StatusCode=StatusCodes.Status400BadRequest});
                return bad;
            }
            var result = await _service.FindPaymentsByFieldsAsync(filters);
            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(new ApiResponse<IEnumerable<PaymentIntent>>{Success=true, Data=result.Data, StatusCode=StatusCodes.Status200OK});
            return ok;
        }
    }
}

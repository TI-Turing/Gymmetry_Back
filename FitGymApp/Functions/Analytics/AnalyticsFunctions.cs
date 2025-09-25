using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO.Analytics;
using Gymmetry.Domain.DTO;
using Gymmetry.Utils;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Gymmetry.Functions.Analytics
{
    public class AnalyticsFunctions
    {
        private readonly ILogger<AnalyticsFunctions> _logger;
        private readonly IAnalyticsService _service;

        public AnalyticsFunctions(ILogger<AnalyticsFunctions> logger, IAnalyticsService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("Analytics_Summary")]
        public async Task<HttpResponseData> SummaryAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "analytics/summary")] HttpRequestData req, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Analytics_Summary");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message=error, StatusCode=StatusCodes.Status401Unauthorized});
                return unauthorized;
            }
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var objRequest = Newtonsoft.Json.JsonConvert.DeserializeObject<AnalyticsSummaryRequest>(requestBody);
            if (objRequest == null || objRequest.UserId != userId || string.IsNullOrWhiteSpace(objRequest.StartDate) || string.IsNullOrWhiteSpace(objRequest.EndDate))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new ApiResponse<object>{Success=false, Message="Solicitud inválida", StatusCode=StatusCodes.Status400BadRequest});
                return bad;
            }
            logger.LogInformation($"Procesando Analytics_Summary para usuario {objRequest.UserId} rango {objRequest.StartDate} - {objRequest.EndDate}");
            try
            {
                var result = await _service.GetSummaryAsync(objRequest);
                if (!result.Success)
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteAsJsonAsync(new ApiResponse<AnalyticsSummaryResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return errorResponse;
                }
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<AnalyticsSummaryResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en Analytics_Summary");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<AnalyticsSummaryResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }
    }
}

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.AppState;
using System;
using System.Net;
using System.Threading.Tasks;
using Gymmetry.Utils;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Gymmetry.Functions.AppStateFunction
{
    public class GetAppStateOverviewFunction
    {
        private readonly ILogger<GetAppStateOverviewFunction> _logger;
        private readonly IAppStateService _appStateService;

        public GetAppStateOverviewFunction(ILogger<GetAppStateOverviewFunction> logger, IAppStateService appStateService)
        {
            _logger = logger;
            _appStateService = appStateService;
        }

        private static void AddCors(HttpRequestData req, HttpResponseData resp)
        {
            string? origin = req.Headers.TryGetValues("Origin", out var ov) ? ov.FirstOrDefault() : null;
            string? reqHeaders = req.Headers.TryGetValues("Access-Control-Request-Headers", out var rh) ? rh.FirstOrDefault() : null;
            CorsHelper.AddCorsHeaders(resp, origin, reqHeaders);
        }

        [Function("AppState_GetOverviewFunction")]
        public async Task<HttpResponseData> GetOverviewAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "app-state/overview")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("AppState_GetOverviewFunction");
            
            // Handle CORS preflight request
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var preflightResponse = req.CreateResponse(HttpStatusCode.NoContent);
                AddCors(req, preflightResponse);
                return preflightResponse;
            }

            logger.LogInformation("Obteniendo estado general de la aplicación");

            // Validate JWT token
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                AddCors(req, unauthorizedResponse);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<AppStateOverviewDto>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            try
            {
                logger.LogInformation($"Obteniendo AppState para usuario: {userId}");
                
                if (!userId.HasValue)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    AddCors(req, badRequestResponse);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<AppStateOverviewDto>
                    {
                        Success = false,
                        Message = "ID de usuario inválido",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badRequestResponse;
                }
                
                var result = await _appStateService.GetAppStateOverviewAsync(userId.Value);

                var statusCode = result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
                var response = req.CreateResponse(statusCode);
                AddCors(req, response);

                await response.WriteAsJsonAsync(new ApiResponse<AppStateOverviewDto>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest
                });

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el estado de la aplicación");
                
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                AddCors(req, errorResponse);
                
                await errorResponse.WriteAsJsonAsync(new ApiResponse<AppStateOverviewDto>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
                
                return errorResponse;
            }
        }
    }
}
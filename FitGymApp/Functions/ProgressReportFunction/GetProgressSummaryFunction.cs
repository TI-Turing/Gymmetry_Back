using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Progress;
using Gymmetry.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gymmetry.Functions.ProgressReportFunction
{
    public class GetProgressSummaryFunction
    {
        private readonly ILogger<GetProgressSummaryFunction> _logger;
        private readonly IProgressReportService _service;

        public GetProgressSummaryFunction(ILogger<GetProgressSummaryFunction> logger, IProgressReportService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("Progress_GetSummaryFunction")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "progress/summary")] HttpRequestData req,
            FunctionContext ctx)
        {
            var logger = ctx.GetLogger("Progress_GetSummaryFunction");
            logger.LogInformation("Procesando solicitud de resumen de progreso");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorized;
            }
            try
            {
                string body = string.Empty;
                try
                {
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                    using var sr = new StreamReader(req.Body, leaveOpen:true);
                    body = await sr.ReadToEndAsync(cts.Token);
                    if (body.Length > 100_000) body = body[..100_000];
                }
                catch (Exception readEx)
                {
                    logger.LogWarning(readEx, "Fallo al leer body summary, usando {}" );
                    body = "{}";
                }

                ProgressReportRequest request;
                try
                {
                    var jo = JObject.Parse(string.IsNullOrWhiteSpace(body)?"{}":body);
                    var userIdToken = jo["UserId"] ?? jo["userId"];
                    if (userIdToken != null && userIdToken.Type == JTokenType.String)
                    {
                        var raw = userIdToken.Value<string>()?.Trim();
                        if (!string.IsNullOrEmpty(raw) && !Guid.TryParse(raw, out _))
                        {
                            userIdToken.Parent.Remove();
                        }
                    }
                    request = jo.ToObject<ProgressReportRequest>() ?? new ProgressReportRequest();
                }
                catch (Exception parseEx)
                {
                    logger.LogWarning(parseEx, "Body JSON inválido en summary, usando request vacío");
                    request = new ProgressReportRequest();
                }
                // Completar UserId desde token si no viene o es Guid.Empty
                if (request.UserId == null || request.UserId == Guid.Empty) request.UserId = userId ?? Guid.Empty;
                // Defaults de fechas si no vienen
                if (string.IsNullOrWhiteSpace(request.StartDate) || string.IsNullOrWhiteSpace(request.EndDate))
                {
                    var today = DateTime.UtcNow.Date;
                    request.EndDate = today.ToString("yyyy-MM-dd");
                    request.StartDate = today.AddDays(-29).ToString("yyyy-MM-dd");
                }
                var result = await _service.GetSummaryAsync(request);
                var ok = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(new ApiResponse<ProgressSummaryResponse>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest
                });
                return ok;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al generar resumen de progreso");
                var errorResp = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResp.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error técnico al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResp;
            }
        }
    }
}

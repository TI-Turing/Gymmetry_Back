using System.IO;using System.Threading.Tasks;using Microsoft.Azure.Functions.Worker;using Microsoft.Azure.Functions.Worker.Http;using Microsoft.Extensions.Logging;using Gymmetry.Application.Services.Interfaces;using Gymmetry.Domain.DTO.Progress;using Gymmetry.Domain.DTO;using Gymmetry.Utils;using Newtonsoft.Json;using Microsoft.AspNetCore.Http;using System.Linq;using Newtonsoft.Json.Linq;

namespace Gymmetry.Functions.ProgressReportFunction
{
    public class GetProgressMultiSummaryFunction
    {
        private readonly ILogger<GetProgressMultiSummaryFunction> _logger;private readonly IProgressReportService _service;
        public GetProgressMultiSummaryFunction(ILogger<GetProgressMultiSummaryFunction> logger, IProgressReportService service){_logger=logger;_service=service;}

        [Function("Progress_GetMultiSummaryFunction")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "progress/summary/multi")] HttpRequestData req,
            FunctionContext ctx)
        {
            var logger = ctx.GetLogger("Progress_GetMultiSummaryFunction");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=StatusCodes.Status401Unauthorized});
                return unauthorized;
            }
            try
            {
                string body = string.Empty;
                try
                {
                    // Defender: si el stream no es seekable y llega vacío / cortado, evitar excepción y continuar con default
                    using var cts = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(5));
                    using var sr = new StreamReader(req.Body, leaveOpen:true);
                    body = await sr.ReadToEndAsync(cts.Token);
                    if (body.Length > 100_000) body = body.Substring(0, 100_000); // límite defensivo
                }
                catch (System.Exception readEx)
                {
                    logger.LogWarning(readEx, "Fallo al leer body, utilizando objeto vacío");
                    body = "{}";
                }

                MultiProgressReportRequest request;
                try
                {
                    var jo = JObject.Parse(string.IsNullOrWhiteSpace(body)?"{}":body);
                    var userIdToken = jo["UserId"] ?? jo["userId"];
                    if (userIdToken != null && userIdToken.Type == JTokenType.String)
                    {
                        var raw = userIdToken.Value<string>()?.Trim();
                        if (!string.IsNullOrEmpty(raw) && !System.Guid.TryParse(raw, out _))
                        {
                            userIdToken.Parent.Remove();
                        }
                    }
                    request = jo.ToObject<MultiProgressReportRequest>() ?? new MultiProgressReportRequest();
                }
                catch (System.Exception parseEx)
                {
                    logger.LogWarning(parseEx, "Body JSON inválido, usando request vacío");
                    request = new MultiProgressReportRequest();
                }

                if (request.UserId == null || request.UserId == System.Guid.Empty) request.UserId = userId ?? System.Guid.Empty;
                if (request.Periods == null || request.Periods.Count==0)
                {
                    var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await bad.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message="Debe enviar Periods", StatusCode=StatusCodes.Status400BadRequest});
                    return bad;
                }
                foreach (var p in request.Periods.Where(p=>!string.IsNullOrWhiteSpace(p.From)&&!string.IsNullOrWhiteSpace(p.To)))
                {
                    if (System.DateTime.TryParse(p.From, out var f)) p.From = f.ToString("yyyy-MM-dd");
                    if (System.DateTime.TryParse(p.To, out var t)) p.To = t.ToString("yyyy-MM-dd");
                }
                if (request.IncludeHistory)
                {
                    var hist = await _service.GetMultiSummaryWithHistoryAsync(request);
                    var resp = req.CreateResponse(System.Net.HttpStatusCode.OK);
                    await resp.WriteAsJsonAsync(new ApiResponse<MultiProgressHistoryResponse>{ Success=hist.Success, Message=hist.Message, Data=hist.Data, StatusCode=hist.Success?StatusCodes.Status200OK:StatusCodes.Status400BadRequest});
                    return resp;
                }
                else
                {
                    var multi = await _service.GetMultiSummaryAsync(request);
                    var resp = req.CreateResponse(System.Net.HttpStatusCode.OK);
                    await resp.WriteAsJsonAsync(new ApiResponse<System.Collections.Generic.List<ProgressSummaryResponse>>{ Success=multi.Success, Message=multi.Message, Data=multi.Data, StatusCode=multi.Success?StatusCodes.Status200OK:StatusCodes.Status400BadRequest});
                    return resp;
                }
            }
            catch(System.Exception ex)
            {
                logger.LogError(ex, "Error multi summary");
                var err = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await err.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message="Error técnico", StatusCode=StatusCodes.Status400BadRequest});
                return err;
            }
        }
    }
}

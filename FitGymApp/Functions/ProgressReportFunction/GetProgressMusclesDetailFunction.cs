using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO.Progress;
using Gymmetry.Domain.DTO;
using Gymmetry.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;

namespace Gymmetry.Functions.ProgressReportFunction
{
    public class GetProgressMusclesDetailFunction
    {
        private readonly ILogger<GetProgressMusclesDetailFunction> _logger;
        private readonly IProgressReportService _service;
        public GetProgressMusclesDetailFunction(ILogger<GetProgressMusclesDetailFunction> logger, IProgressReportService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("Progress_GetMusclesDetailFunction")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "progress/muscles")] HttpRequestData req,
            FunctionContext ctx)
        {
            var logger = ctx.GetLogger("Progress_GetMusclesDetailFunction");
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
                    using var cts = new System.Threading.CancellationTokenSource(System.TimeSpan.FromSeconds(5));
                    using var sr = new StreamReader(req.Body, leaveOpen:true);
                    body = await sr.ReadToEndAsync(cts.Token);
                    if (body.Length>100_000) body = body[..100_000];
                }
                catch (System.Exception readEx)
                {
                    logger.LogWarning(readEx, "Fallo al leer body muscles, usando {}" );
                    body = "{}";
                }
                ProgressReportRequest request;
                try
                {
                    var jo = JObject.Parse(string.IsNullOrWhiteSpace(body)?"{}":body);
                    var userIdToken = jo["UserId"] ?? jo["userId"];
                    if (userIdToken!=null && userIdToken.Type==JTokenType.String)
                    {
                        var raw = userIdToken.Value<string>()?.Trim();
                        if (!string.IsNullOrEmpty(raw) && !System.Guid.TryParse(raw, out _)) userIdToken.Parent.Remove();
                    }
                    request = jo.ToObject<ProgressReportRequest>() ?? new ProgressReportRequest();
                }
                catch(System.Exception parseEx)
                {
                    logger.LogWarning(parseEx, "JSON inválido en muscles, request vacío");
                    request = new ProgressReportRequest();
                }
                if (request.UserId == null || request.UserId == System.Guid.Empty) request.UserId = userId ?? System.Guid.Empty;
                if (string.IsNullOrWhiteSpace(request.StartDate) || string.IsNullOrWhiteSpace(request.EndDate))
                {
                    var today = System.DateTime.UtcNow.Date;
                    request.EndDate = today.ToString("yyyy-MM-dd");
                    request.StartDate = today.AddDays(-29).ToString("yyyy-MM-dd");
                }
                var result = await _service.GetMusclesDetailAsync(request);
                var ok = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await ok.WriteAsJsonAsync(new ApiResponse<MusclesDetailResponse>{ Success = result.Success, Message = result.Message, Data = result.Data, StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest });
                return ok;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error detalle músculos");
                var err = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await err.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message="Error técnico", StatusCode=StatusCodes.Status400BadRequest});
                return err;
            }
        }
    }
}

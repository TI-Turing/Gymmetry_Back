using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Functions.FeedFunction
{
    public class AutoPostMilestoneFunction
    {
        private readonly ILogger<AutoPostMilestoneFunction> _logger;
        private readonly IFeedService _feedService;
        public AutoPostMilestoneFunction(ILogger<AutoPostMilestoneFunction> logger, IFeedService feedService)
        { _logger = logger; _feedService = feedService; }

        private record MilestoneRequest(Guid? UserId, string MilestoneCode, string? Extra);

        [Function("Feed_AutoPostMilestone")] 
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "feed/milestone")] HttpRequestData req,
            FunctionContext ctx)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var tokenUser))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=401 });
                return unauthorized;
            }
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var payload = JsonSerializer.Deserialize<MilestoneRequest>(body) ?? new MilestoneRequest(null, string.Empty, null);
            if (string.IsNullOrWhiteSpace(payload.MilestoneCode))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message="MilestoneCode requerido", StatusCode=400 });
                return bad;
            }
            var userId = payload.UserId ?? tokenUser ?? Guid.Empty;
            var result = await _feedService.AutoPostProgressMilestoneAsync(userId, payload.MilestoneCode, payload.Extra);
            var resp = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await resp.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success?200:400 });
            return resp;
        }
    }
}

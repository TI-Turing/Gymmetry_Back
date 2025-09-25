using System;
using System.IO;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Functions.FeedFunction
{
    public class FeedLikeFunctions
    {
        private readonly IFeedService _feedService;
        private readonly ILogger<FeedLikeFunctions> _logger;
        public FeedLikeFunctions(IFeedService feedService, ILogger<FeedLikeFunctions> logger) { _feedService = feedService; _logger = logger; }

        [Function("Feed_Like")] // POST feed/{feedId}/like
        public async Task<HttpResponseData> LikeAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "feed/{feedId:guid}/like")] HttpRequestData req,
            Guid feedId,
            FunctionContext ctx)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var una = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await una.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=StatusCodes.Status401Unauthorized });
                return una;
            }
            try
            {
                string? ip = req.Headers.TryGetValues("X-Forwarded-For", out var v) ? v.FirstOrDefault() : null;
                var result = await _feedService.LikeAsync(feedId, userId!.Value, ip);
                var resp = req.CreateResponse(result.Success ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest);
                await resp.WriteAsJsonAsync(new ApiResponse<bool>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success?StatusCodes.Status200OK:StatusCodes.Status400BadRequest });
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error like feed");
                var err = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await err.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message="Error técnico", StatusCode=StatusCodes.Status400BadRequest });
                return err;
            }
        }

        [Function("Feed_Unlike")] // DELETE feed/{feedId}/like
        public async Task<HttpResponseData> UnlikeAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "feed/{feedId:guid}/like")] HttpRequestData req,
            Guid feedId,
            FunctionContext ctx)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var una = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await una.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=StatusCodes.Status401Unauthorized });
                return una;
            }
            try
            {
                var result = await _feedService.UnlikeAsync(feedId, userId!.Value);
                var resp = req.CreateResponse(result.Success ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest);
                await resp.WriteAsJsonAsync(new ApiResponse<bool>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success?StatusCodes.Status200OK:StatusCodes.Status400BadRequest });
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlike feed");
                var err = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await err.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message="Error técnico", StatusCode=StatusCodes.Status400BadRequest });
                return err;
            }
        }

        [Function("Feed_LikesCount")] // GET feed/{feedId}/likes/count
        public async Task<HttpResponseData> LikesCountAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feed/{feedId:guid}/likes/count")] HttpRequestData req,
            Guid feedId,
            FunctionContext ctx)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var _))
            {
                var una = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await una.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=StatusCodes.Status401Unauthorized });
                return una;
            }
            var result = await _feedService.GetLikesCountAsync(feedId);
            var rsp = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await rsp.WriteAsJsonAsync(new ApiResponse<int>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode=StatusCodes.Status200OK });
            return rsp;
        }
    }
}

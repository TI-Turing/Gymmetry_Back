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
    public class GetPagedFeedFunction
    {
        private readonly ILogger<GetPagedFeedFunction> _logger;
        private readonly IFeedService _feedService;
        public GetPagedFeedFunction(ILogger<GetPagedFeedFunction> logger, IFeedService feedService)
        { _logger = logger; _feedService = feedService; }

        private static (int page, int size) ParsePaging(HttpRequestData req)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            int page = int.TryParse(query.Get("page"), out var p) ? p : 1;
            int size = int.TryParse(query.Get("size"), out var s) ? s : 20;
            return (page, size);
        }

        [Function("Feed_GetGlobalPaged")] 
        public async Task<HttpResponseData> GetGlobalAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "feed/paged")] HttpRequestData req,
            FunctionContext ctx)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=401 });
                return unauthorized;
            }
            var (page, size) = ParsePaging(req);
            var result = await _feedService.GetGlobalFeedPagedAsync(page, size, userId);
            var resp = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await resp.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success?200:400 });
            return resp;
        }

        [Function("Feed_GetUserPaged")] 
        public async Task<HttpResponseData> GetUserAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "feed/user/{userId:guid}/paged")] HttpRequestData req,
            Guid userId,
            FunctionContext ctx)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var viewer))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=401 });
                return unauthorized;
            }
            var (page, size) = ParsePaging(req);
            var result = await _feedService.GetUserFeedPagedAsync(userId, page, size, viewer);
            var resp = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await resp.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success?200:400 });
            return resp;
        }

        [Function("Feed_GetTrending")] 
        public async Task<HttpResponseData> GetTrendingAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "feed/trending")] HttpRequestData req,
            FunctionContext ctx)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var viewer))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=401 });
                return unauthorized;
            }
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            int hours = int.TryParse(query.Get("hours"), out var h) ? h : 24;
            int take = int.TryParse(query.Get("take"), out var t) ? t : 20;
            var result = await _feedService.GetTrendingFeedAsync(hours, take);
            var resp = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await resp.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success?200:400 });
            return resp;
        }
    }
}

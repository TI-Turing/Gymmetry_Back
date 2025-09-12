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
using Newtonsoft.Json;

namespace Gymmetry.Functions.FeedFunction
{
    public class FeedCommentFunctions
    {
        private readonly IFeedService _feedService;
        private readonly ILogger<FeedCommentFunctions> _logger;
        public FeedCommentFunctions(IFeedService feedService, ILogger<FeedCommentFunctions> logger) { _feedService = feedService; _logger = logger; }

        private record AddCommentPayload(string Content, bool IsAnonymous);

        [Function("Feed_AddComment")] // POST feed/{feedId}/comment
        public async Task<HttpResponseData> AddCommentAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "feed/{feedId:guid}/comment")] HttpRequestData req,
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
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var payload = JsonConvert.DeserializeObject<AddCommentPayload>(body) ?? new AddCommentPayload(string.Empty, false);
                string? ip = req.Headers.TryGetValues("X-Forwarded-For", out var v) ? v.FirstOrDefault() : null;
                var result = await _feedService.AddCommentAsync(feedId, userId!.Value, payload.Content, payload.IsAnonymous, ip);
                var resp = req.CreateResponse(result.Success ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest);
                await resp.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success?StatusCodes.Status200OK:StatusCodes.Status400BadRequest });
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error add comment");
                var err = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await err.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message="Error técnico", StatusCode=StatusCodes.Status400BadRequest });
                return err;
            }
        }

        [Function("Feed_DeleteComment")] // DELETE feed/comment/{commentId}
        public async Task<HttpResponseData> DeleteCommentAsync(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "feed/comment/{commentId:guid}")] HttpRequestData req,
            Guid commentId,
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
                var result = await _feedService.DeleteCommentAsync(commentId, userId!.Value);
                var resp = req.CreateResponse(result.Success ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.BadRequest);
                await resp.WriteAsJsonAsync(new ApiResponse<bool>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success?StatusCodes.Status200OK:StatusCodes.Status400BadRequest });
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error delete comment");
                var err = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await err.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message="Error técnico", StatusCode=StatusCodes.Status400BadRequest });
                return err;
            }
        }

        [Function("Feed_GetComments")] // GET feed/{feedId}/comments?page=1&size=50
        public async Task<HttpResponseData> GetCommentsAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "feed/{feedId:guid}/comments")] HttpRequestData req,
            Guid feedId,
            FunctionContext ctx)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var _))
            {
                var una = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await una.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=StatusCodes.Status401Unauthorized });
                return una;
            }
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            int page = int.TryParse(query.Get("page"), out var p) ? p : 1;
            int size = int.TryParse(query.Get("size"), out var s) ? s : 50;
            var result = await _feedService.GetCommentsAsync(feedId, page, size);
            var resp = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await resp.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode=StatusCodes.Status200OK });
            return resp;
        }

        [Function("Feed_CommentsCount")] // GET feed/{feedId}/comments/count
        public async Task<HttpResponseData> GetCommentsCountAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "feed/{feedId:guid}/comments/count")] HttpRequestData req,
            Guid feedId,
            FunctionContext ctx)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var _))
            {
                var una = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await una.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=StatusCodes.Status401Unauthorized });
                return una;
            }
            var result = await _feedService.GetCommentsCountAsync(feedId);
            var resp = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await resp.WriteAsJsonAsync(new ApiResponse<int>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode=StatusCodes.Status200OK });
            return resp;
        }
    }
}

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using System.Text.Json;
using Gymmetry.Domain.DTO.Feed.Request;
using Gymmetry.Domain.DTO;
using Gymmetry.Utils;

namespace Gymmetry.Functions.FeedFunction
{
    public class GetFeedFunction
    {
        private readonly ILogger<GetFeedFunction> _logger;
        private readonly IFeedService _feedService;

        public GetFeedFunction(ILogger<GetFeedFunction> logger, IFeedService feedService)
        {
            _logger = logger;
            _feedService = feedService;
        }

        [Function("Feed_GetFeedByIdFunction")]
        public async Task<HttpResponseData> GetFeedByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feed/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=401 });
                return unauthorized;
            }
            var result = await _feedService.GetFeedByIdAsync(id);
            var status = result.Success ? HttpStatusCode.OK : HttpStatusCode.NotFound;
            var resp = req.CreateResponse(status);
            await resp.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode=(int)status });
            return resp;
        }

        [Function("Feed_GetAllFeedsFunction")]
        public async Task<HttpResponseData> GetAllFeedsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feed")] HttpRequestData req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=401 });
                return unauthorized;
            }
            var result = await _feedService.GetAllFeedsAsync();
            var resp = req.CreateResponse(HttpStatusCode.OK);
            await resp.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode=200 });
            return resp;
        }

        [Function("Feed_SearchFeedsFunction")]
        public async Task<HttpResponseData> SearchFeedsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "feed/search")] HttpRequestData req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorized = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorized.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=401 });
                return unauthorized;
            }
            _logger.LogInformation("Processing request to search feeds");
            var body = await req.ReadAsStringAsync();
            var searchRequest = JsonSerializer.Deserialize<SearchFeedRequest>(body);
            if (searchRequest == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message="Datos de búsqueda inválidos.", StatusCode=400 });
                return badRequest;
            }
            var result = await _feedService.SearchFeedsAsync(searchRequest.Title, searchRequest.Description, searchRequest.UserId, searchRequest.Hashtag, searchRequest.PageNumber, searchRequest.PageSize);
            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success ? 200 : 400 });
            return response;
        }
    }
}

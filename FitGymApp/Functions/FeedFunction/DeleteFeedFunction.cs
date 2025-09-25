using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Utils;

namespace Gymmetry.Functions.FeedFunction
{
    public class DeleteFeedFunction
    {
        private readonly ILogger<DeleteFeedFunction> _logger;
        private readonly IFeedService _feedService;

        public DeleteFeedFunction(ILogger<DeleteFeedFunction> logger, IFeedService feedService)
        {
            _logger = logger;
            _feedService = feedService;
        }

        [Function("Feed_DeleteFeedFunction")]
        public async Task<HttpResponseData> DeleteFeedAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "feed/{id:guid}")] HttpRequestData req,
            Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=401 });
                return unauthorizedResponse;
            }
            var result = await _feedService.DeleteFeedAsync(id);
            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success ? 200 : 400 });
            return response;
        }
    }
}

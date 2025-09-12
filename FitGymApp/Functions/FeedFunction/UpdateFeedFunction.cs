using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Feed.Request;
using Gymmetry.Utils;
using System.Text.Json;
using Gymmetry.Domain.Models;

namespace Gymmetry.Functions.FeedFunction
{
    public class UpdateFeedFunction
    {
        private readonly ILogger<UpdateFeedFunction> _logger;
        private readonly IFeedService _feedService;

        public UpdateFeedFunction(ILogger<UpdateFeedFunction> logger, IFeedService feedService)
        {
            _logger = logger;
            _feedService = feedService;
        }

        [Function("Feed_UpdateFeedFunction")]
        public async Task<HttpResponseData> UpdateFeedAsync(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "feed/{id:guid}")] HttpRequestData req,
            Guid id,
            FunctionContext executionContext)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<object>{ Success=false, Message=error!, StatusCode=401 });
                return unauthorizedResponse;
            }

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var dto = JsonSerializer.Deserialize<FeedUpdateRequestDto>(body) ?? new FeedUpdateRequestDto();
            var entity = new Feed { Id = id, Title = dto.Title ?? string.Empty, Description = dto.Description ?? string.Empty };
            var ip = FunctionResponseHelper.GetClientIp(req);
            var result = await _feedService.UpdateFeedAsync(entity, dto.Media, dto.FileName, dto.MediaType, userId, ip, executionContext.InvocationId);
            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success ? 200 : 400 });
            return response;
        }
    }
}

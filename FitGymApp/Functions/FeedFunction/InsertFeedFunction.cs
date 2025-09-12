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
    public class InsertFeedFunction
    {
        private readonly ILogger<InsertFeedFunction> _logger;
        private readonly IFeedService _feedService;

        public InsertFeedFunction(ILogger<InsertFeedFunction> logger, IFeedService feedService)
        {
            _logger = logger;
            _feedService = feedService;
        }

        [Function("Feed_CreateFeedFunction")]
        public async Task<HttpResponseData> CreateFeedAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "feed")] HttpRequestData req,
            FunctionContext executionContext)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = 401
                });
                return unauthorizedResponse;
            }

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var dto = JsonSerializer.Deserialize<FeedCreateRequestDto>(body);
            if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Datos de feed inválidos.",
                    Data = null,
                    StatusCode = 400
                });
                return badRequest;
            }
            var entity = new Feed { UserId = userId ?? Guid.Empty, Title = dto.Title, Description = dto.Description };
            var result = await _feedService.CreateFeedAsync(entity, dto.Media, dto.FileName, dto.MediaType);
            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success ? 200 : 400 });
            return response;
        }

        [Function("Feed_UploadMediaFunction")]
        public async Task<HttpResponseData> UploadFeedMediaAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "feed/upload-media")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Feed_UploadMediaFunction");
            logger.LogInformation("Processing request to upload feed media");

            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = 401
                });
                return unauthorizedResponse;
            }

            string? contentType = req.Headers.TryGetValues("Content-Type", out var ctValues) ? ctValues.FirstOrDefault() : null;
            var boundary = MultipartRequestHelper.GetBoundary(contentType ?? string.Empty, 4096);
            var sections = await MultipartRequestHelper.GetSectionsAsync(req.Body, boundary);
            var feedId = Guid.Empty;
            byte[]? mediaBytes = null;
            string? fileName = null;
            string? mediaType = null;

            foreach (var section in sections)
            {
                if (section.Name == "FeedId")
                {
                    feedId = Guid.Parse(section.Value);
                }
                if (section.Name == "Media" && section.FileContent != null)
                {
                    mediaBytes = section.FileContent;
                    fileName = section.FileName;
                    mediaType = section.ContentType;
                }
            }

            if (feedId == Guid.Empty || mediaBytes == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = "FeedId y Media son requeridos.",
                    Data = null,
                    StatusCode = 400
                });
                return badResponse;
            }

            var result = await _feedService.UploadFeedMediaAsync(feedId, mediaBytes, fileName, mediaType);
            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
            await response.WriteAsJsonAsync(new ApiResponse<object>{ Success=result.Success, Message=result.Message, Data=result.Data, StatusCode = result.Success ? 200 : 400 });
            return response;
        }
    }
}

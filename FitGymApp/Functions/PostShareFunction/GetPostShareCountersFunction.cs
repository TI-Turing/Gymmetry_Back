using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Web;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.PostShare.Response;
using Gymmetry.Utils;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Gymmetry.Functions.PostShareFunction
{
    public class GetPostShareCountersFunction
    {
        private readonly IPostShareService _postShareService;
        private readonly ILogger<GetPostShareCountersFunction> _logger;

        public GetPostShareCountersFunction(IPostShareService postShareService, ILogger<GetPostShareCountersFunction> logger)
        {
            _postShareService = postShareService;
            _logger = logger;
        }

        [Function("PostShare_GetPostShareCountersFunction")]
        public async Task<HttpResponseData> GetCountersAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "postShare/counters")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("PostShare_GetPostShareCountersFunction");
            logger.LogInformation("Procesando solicitud para obtener contadores de PostShare.");

            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<PostShareCountersResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            try
            {
                var query = HttpUtility.ParseQueryString(req.Url.Query);
                var postIdString = query["postId"];
                
                if (string.IsNullOrEmpty(postIdString) || !Guid.TryParse(postIdString, out var postId))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<PostShareCountersResponse>
                    {
                        Success = false,
                        Message = "postId inválido o faltante en query parameter.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badRequestResponse;
                }

                var result = await _postShareService.GetPostShareCountersAsync(postId);

                var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new ApiResponse<PostShareCountersResponse>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest
                });

                logger.LogInformation("Contadores de PostShare obtenidos exitosamente para PostId: {PostId}", postId);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener contadores de PostShare");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<PostShareCountersResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
                return errorResponse;
            }
        }
    }
}
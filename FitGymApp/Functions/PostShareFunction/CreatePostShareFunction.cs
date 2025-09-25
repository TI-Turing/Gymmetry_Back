using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.PostShare.Request;
using Gymmetry.Domain.DTO.PostShare.Response;
using Gymmetry.Utils;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Gymmetry.Functions.PostShareFunction
{
    public class CreatePostShareFunction
    {
        private readonly IPostShareService _postShareService;
        private readonly ILogger<CreatePostShareFunction> _logger;

        public CreatePostShareFunction(IPostShareService postShareService, ILogger<CreatePostShareFunction> logger)
        {
            _postShareService = postShareService;
            _logger = logger;
        }

        [Function("PostShare_CreatePostShareFunction")]
        public async Task<HttpResponseData> CreateAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "postShare/add")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("PostShare_CreatePostShareFunction");
            logger.LogInformation("Procesando solicitud para crear PostShare.");

            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<PostShareResponse>
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
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<AddPostShareRequest>(requestBody);
                
                if (request == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<PostShareResponse>
                    {
                        Success = false,
                        Message = "Request body inválido.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badRequestResponse;
                }

                // Asegurar que SharedBy coincida con el usuario autenticado
                request.SharedBy = userId.Value;

                // Obtener IP del request
                string? ip = null;
                if (req.Headers.TryGetValues("X-Forwarded-For", out var forwardedFor))
                {
                    ip = forwardedFor.FirstOrDefault()?.Split(',')[0].Trim();
                }
                else if (req.Headers.TryGetValues("X-Real-IP", out var realIp))
                {
                    ip = realIp.FirstOrDefault();
                }

                var result = await _postShareService.CreatePostShareAsync(request, ip ?? "");

                var responseCode = result.Success ? HttpStatusCode.Created : 
                                 result.ErrorCode == "RateLimitExceeded" ? (HttpStatusCode)429 : 
                                 HttpStatusCode.BadRequest;

                var response = req.CreateResponse(responseCode);
                await response.WriteAsJsonAsync(new ApiResponse<PostShareResponse>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status201Created : 
                                result.ErrorCode == "RateLimitExceeded" ? 429 : StatusCodes.Status400BadRequest
                });

                logger.LogInformation("PostShare creado exitosamente para usuario {UserId}", userId);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear PostShare");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<PostShareResponse>
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
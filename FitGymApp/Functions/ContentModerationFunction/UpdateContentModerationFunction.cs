using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ContentModeration;
using Gymmetry.Repository.Services.Interfaces;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Gymmetry.Utils;
using FitGymApp.Utils;

namespace Gymmetry.Functions.ContentModerationFunction
{
    public class UpdateContentModerationFunction
    {
        private readonly ILogger<UpdateContentModerationFunction> _logger;
        private readonly IContentModerationService _service;
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;

        public UpdateContentModerationFunction(ILogger<UpdateContentModerationFunction> logger, IContentModerationService service, IUserRepository userRepository, IUserTypeRepository userTypeRepository)
        {
            _logger = logger;
            _service = service;
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
        }

        [Function("ContentModeration_UpdateContentModerationFunction")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "contentmoderation/{id:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid id)
        {
            var logger = executionContext.GetLogger("ContentModeration_UpdateContentModerationFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = error!,
                    Data = false,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            var isModerator = await ModeratorValidator.IsModeratorAsync(userId, _userRepository, _userTypeRepository);
            if (!isModerator)
            {
                var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Acceso restringido",
                    Data = false,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation($"Procesando solicitud de actualización para ContentModeration {id}");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var objRequest = JsonSerializer.Deserialize<ContentModerationUpdateRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (objRequest == null)
                {
                    var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Payload inválido",
                        Data = false,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badRequestResponse;
                }

                objRequest.Id = id;
                var ip = FunctionResponseHelper.GetClientIp(req);
                var result = await _service.UpdateAsync(objRequest, userId, ip);
                
                if (!result.Success)
                {
                    var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await errorResponse.WriteAsJsonAsync(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = false,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return errorResponse;
                }

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar ContentModeration.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = false,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }
    }
}
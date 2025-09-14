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
    public class AddContentModerationFunction
    {
        private readonly ILogger<AddContentModerationFunction> _logger;
        private readonly IContentModerationService _service;
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;

        public AddContentModerationFunction(ILogger<AddContentModerationFunction> logger, IContentModerationService service, IUserRepository userRepository, IUserTypeRepository userTypeRepository)
        {
            _logger = logger;
            _service = service;
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
        }

        [Function("ContentModeration_AddContentModerationFunction")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "contentmoderation")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ContentModeration_AddContentModerationFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation("Procesando solicitud de creación de moderación");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var objRequest = JsonSerializer.Deserialize<ContentModerationCreateRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var validation = ModelValidator.ValidateModel<ContentModerationCreateRequest, ContentModerationResponse>(objRequest, StatusCodes.Status400BadRequest);
                if (validation != null)
                {
                    var validationResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await validationResponse.WriteAsJsonAsync(validation);
                    return validationResponse;
                }

                // Validar permisos para moderación manual
                if (!objRequest!.AutoModerated)
                {
                    var isModerator = await ModeratorValidator.IsModeratorAsync(userId, _userRepository, _userTypeRepository);
                    if (!isModerator)
                    {
                        var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                        await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
                        {
                            Success = false,
                            Message = "Solo moderadores pueden crear moderación manual",
                            Data = null,
                            StatusCode = StatusCodes.Status403Forbidden
                        });
                        return forbiddenResponse;
                    }
                }

                var ip = FunctionResponseHelper.GetClientIp(req);
                var result = await _service.CreateAsync(objRequest!, userId, ip);
                
                if (!result.Success)
                {
                    var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await errorResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return errorResponse;
                }

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
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
                logger.LogError(ex, "Error al crear moderación");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ContentModeration;
using Gymmetry.Repository.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Gymmetry.Utils;
using FitGymApp.Utils;

namespace Gymmetry.Functions.ContentModerationFunction
{
    public class ContentModerationBulkFunction
    {
        private readonly ILogger<ContentModerationBulkFunction> _logger;
        private readonly IContentModerationService _service;
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;

        public ContentModerationBulkFunction(ILogger<ContentModerationBulkFunction> logger, IContentModerationService service, IUserRepository userRepository, IUserTypeRepository userTypeRepository)
        {
            _logger = logger;
            _service = service;
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
        }

        [Function("ContentModeration_BulkApproveFunction")]
        public async Task<HttpResponseData> BulkApproveAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "contentmoderation/bulk/approve")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ContentModeration_BulkApproveFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<BulkModerationResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            if (!userId.HasValue)
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<BulkModerationResponse>
                {
                    Success = false,
                    Message = "Usuario no identificado",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            var isModerator = await ModeratorValidator.IsModeratorAsync(userId, _userRepository, _userTypeRepository);
            if (!isModerator)
            {
                var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<BulkModerationResponse>
                {
                    Success = false,
                    Message = "Acceso restringido",
                    Data = null,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation($"Procesando aprobación masiva para moderador: {userId.Value}");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var objRequest = JsonSerializer.Deserialize<BulkModerationRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var validation = ModelValidator.ValidateModel<BulkModerationRequest, BulkModerationResponse>(objRequest, StatusCodes.Status400BadRequest);
                if (validation != null)
                {
                    var validationResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await validationResponse.WriteAsJsonAsync(validation);
                    return validationResponse;
                }

                var result = await _service.BulkApproveAsync(objRequest!, userId.Value);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<BulkModerationResponse>
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
                logger.LogError(ex, "Error en aprobación masiva");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<BulkModerationResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("ContentModeration_BulkRejectFunction")]
        public async Task<HttpResponseData> BulkRejectAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "contentmoderation/bulk/reject")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ContentModeration_BulkRejectFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<BulkModerationResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            if (!userId.HasValue)
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<BulkModerationResponse>
                {
                    Success = false,
                    Message = "Usuario no identificado",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            var isModerator = await ModeratorValidator.IsModeratorAsync(userId, _userRepository, _userTypeRepository);
            if (!isModerator)
            {
                var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<BulkModerationResponse>
                {
                    Success = false,
                    Message = "Acceso restringido",
                    Data = null,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation($"Procesando rechazo masivo para moderador: {userId.Value}");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var objRequest = JsonSerializer.Deserialize<BulkModerationRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var validation = ModelValidator.ValidateModel<BulkModerationRequest, BulkModerationResponse>(objRequest, StatusCodes.Status400BadRequest);
                if (validation != null)
                {
                    var validationResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await validationResponse.WriteAsJsonAsync(validation);
                    return validationResponse;
                }

                var result = await _service.BulkRejectAsync(objRequest!, userId.Value);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<BulkModerationResponse>
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
                logger.LogError(ex, "Error en rechazo masivo");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<BulkModerationResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("ContentModeration_AutoScanFunction")]
        public async Task<HttpResponseData> AutoScanAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "contentmoderation/auto-scan")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ContentModeration_AutoScanFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<AutoScanResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            var isModerator = await ModeratorValidator.IsModeratorAsync(userId, _userRepository, _userTypeRepository);
            if (!isModerator)
            {
                var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<AutoScanResponse>
                {
                    Success = false,
                    Message = "Acceso restringido",
                    Data = null,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation("Ejecutando escaneo automático de contenido");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var objRequest = JsonSerializer.Deserialize<AutoScanRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new AutoScanRequest();

                var result = await _service.AutoScanAsync(objRequest);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<AutoScanResponse>
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
                logger.LogError(ex, "Error en escaneo automático");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<AutoScanResponse>
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
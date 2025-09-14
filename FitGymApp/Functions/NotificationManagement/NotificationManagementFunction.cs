using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.UnifiedNotification;
using Gymmetry.Utils;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gymmetry.Functions.NotificationManagement
{
    public class NotificationManagementFunction
    {
        private readonly ILogger<NotificationManagementFunction> _logger;
        private readonly IUnifiedNotificationService _unifiedService;

        public NotificationManagementFunction(ILogger<NotificationManagementFunction> logger, IUnifiedNotificationService unifiedService)
        {
            _logger = logger;
            _unifiedService = unifiedService;
        }

        [Function("NotificationManagement_SendUnifiedFunction")]
        public async Task<HttpResponseData> SendUnifiedAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications/management/send")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("NotificationManagement_SendUnifiedFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<NotificationDeliveryResult>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation("Procesando envío de notificación unificada");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<UnifiedNotificationRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var validation = ModelValidator.ValidateModel<UnifiedNotificationRequest, NotificationDeliveryResult>(request, StatusCodes.Status400BadRequest);
                if (validation != null)
                {
                    var validationResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await validationResponse.WriteAsJsonAsync(validation);
                    return validationResponse;
                }

                var result = await _unifiedService.SendUnifiedNotificationAsync(request!);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<NotificationDeliveryResult>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error enviando notificación unificada");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<NotificationDeliveryResult>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("NotificationManagement_GetUserPreferencesFunction")]
        public async Task<HttpResponseData> GetUserPreferencesAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notifications/management/user/{userId:guid}/preferences")] HttpRequestData req,
            FunctionContext executionContext,
            Guid userId)
        {
            var logger = executionContext.GetLogger("NotificationManagement_GetUserPreferencesFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var tokenUserId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            // Validar que el usuario solo pueda ver sus propias preferencias
            if (tokenUserId != userId)
            {
                var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No tienes permisos para ver estas preferencias",
                    Data = null,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation($"Consultando preferencias de notificación para usuario: {userId}");
            
            try
            {
                var result = await _unifiedService.GetUserPreferencesAsync(userId);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status404NotFound
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consultando preferencias de usuario");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("NotificationManagement_UpdateUserPreferencesFunction")]
        public async Task<HttpResponseData> UpdateUserPreferencesAsync(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "notifications/management/user/{userId:guid}/preferences")] HttpRequestData req,
            FunctionContext executionContext,
            Guid userId)
        {
            var logger = executionContext.GetLogger("NotificationManagement_UpdateUserPreferencesFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var tokenUserId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            // Validar que el usuario solo pueda actualizar sus propias preferencias
            if (tokenUserId != userId)
            {
                var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = false,
                    Message = "No tienes permisos para actualizar estas preferencias",
                    Data = null,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation($"Actualizando preferencias de notificación para usuario: {userId}");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<UserPreferencesRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (request == null)
                {
                    var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Request inválido",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badRequestResponse;
                }

                // Asegurar que el UserId del request coincida con el de la URL
                request.UserId = userId;

                var result = await _unifiedService.UpdateUserPreferencesAsync(request);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<object>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error actualizando preferencias de usuario");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<object>
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
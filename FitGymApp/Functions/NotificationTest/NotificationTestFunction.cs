using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.UnifiedNotification;
using Gymmetry.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gymmetry.Functions.NotificationTest
{
    public class NotificationTestFunction
    {
        private readonly ILogger<NotificationTestFunction> _logger;
        private readonly IUnifiedNotificationService _unifiedService;

        public NotificationTestFunction(ILogger<NotificationTestFunction> logger, IUnifiedNotificationService unifiedService)
        {
            _logger = logger;
            _unifiedService = unifiedService;
        }

        [Function("NotificationTest_SendTestNotificationFunction")]
        public async Task<HttpResponseData> SendTestNotificationAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notifications/test/send")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("NotificationTest_SendTestNotificationFunction");
            
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

            // Validar que userId no sea null
            if (!userId.HasValue)
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<NotificationDeliveryResult>
                {
                    Success = false,
                    Message = "Token JWT inválido - UserId no encontrado",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            logger.LogInformation("Enviando notificación de prueba para usuario: {UserId}", userId.Value);
            
            try
            {
                // Crear notificación de prueba
                var testRequest = new UnifiedNotificationRequest
                {
                    UserId = userId.Value,
                    NotificationType = "fitness",
                    Priority = "normal",
                    TemplateKey = "workout_reminder",
                    TemplateData = new Dictionary<string, object>(),
                    SkipPreferences = false
                };

                var result = await _unifiedService.SendUnifiedNotificationAsync(testRequest);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<NotificationDeliveryResult>
                {
                    Success = result.Success,
                    Message = result.Success ? "Notificación de prueba enviada correctamente" : result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error enviando notificación de prueba");
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

        [Function("NotificationTest_CreateDefaultPreferencesFunction")]
        public async Task<HttpResponseData> CreateDefaultPreferencesAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "notifications/test/preferences/default")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("NotificationTest_CreateDefaultPreferencesFunction");
            
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

            // Validar que userId no sea null
            if (!userId.HasValue)
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Token JWT inválido - UserId no encontrado",
                    Data = false,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            logger.LogInformation("Creando preferencias por defecto para usuario: {UserId}", userId.Value);
            
            try
            {
                var notificationTypes = new[] { "fitness", "social", "billing", "security", "moderation" };
                var createdCount = 0;

                foreach (var type in notificationTypes)
                {
                    var preferenceRequest = new UserPreferencesRequest
                    {
                        UserId = userId.Value,
                        NotificationType = type,
                        PushEnabled = true,
                        AppEnabled = true,
                        EmailEnabled = type == "billing" || type == "security",
                        SmsEnabled = type == "security",
                        WhatsAppEnabled = false
                    };

                    var result = await _unifiedService.UpdateUserPreferencesAsync(preferenceRequest);
                    if (result.Success) createdCount++;
                }

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"Se crearon {createdCount} preferencias por defecto",
                    Data = true,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creando preferencias por defecto");
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
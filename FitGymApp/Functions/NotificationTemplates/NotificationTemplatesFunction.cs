using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.UnifiedNotification;
using Gymmetry.Domain.Models;
using Gymmetry.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gymmetry.Functions.NotificationTemplates
{
    public class NotificationTemplatesFunction
    {
        private readonly ILogger<NotificationTemplatesFunction> _logger;
        private readonly IUnifiedNotificationService _unifiedService;

        public NotificationTemplatesFunction(ILogger<NotificationTemplatesFunction> logger, IUnifiedNotificationService unifiedService)
        {
            _logger = logger;
            _unifiedService = unifiedService;
        }

        [Function("NotificationTemplates_GetAllFunction")]
        public async Task<HttpResponseData> GetAllAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notifications/templates")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("NotificationTemplates_GetAllFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<NotificationTemplate>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation("Consultando todos los templates de notificación");
            
            try
            {
                var result = await _unifiedService.GetTemplatesAsync();

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<NotificationTemplate>>
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
                logger.LogError(ex, "Error consultando templates");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<NotificationTemplate>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("NotificationTemplates_CreateFunction")]
        public async Task<HttpResponseData> CreateAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications/templates")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("NotificationTemplates_CreateFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<NotificationTemplate>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation("Creando nuevo template de notificación");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<NotificationTemplateRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var validation = ModelValidator.ValidateModel<NotificationTemplateRequest, NotificationTemplate>(request, StatusCodes.Status400BadRequest);
                if (validation != null)
                {
                    var validationResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await validationResponse.WriteAsJsonAsync(validation);
                    return validationResponse;
                }

                var result = await _unifiedService.CreateTemplateAsync(request!);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<NotificationTemplate>
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
                logger.LogError(ex, "Error creando template");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<NotificationTemplate>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("NotificationTemplates_UpdateFunction")]
        public async Task<HttpResponseData> UpdateAsync(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "notifications/templates/{templateId:int}")] HttpRequestData req,
            FunctionContext executionContext,
            int templateId)
        {
            var logger = executionContext.GetLogger("NotificationTemplates_UpdateFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<NotificationTemplate>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation($"Actualizando template de notificación: {templateId}");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<NotificationTemplateRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var validation = ModelValidator.ValidateModel<NotificationTemplateRequest, NotificationTemplate>(request, StatusCodes.Status400BadRequest);
                if (validation != null)
                {
                    var validationResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await validationResponse.WriteAsJsonAsync(validation);
                    return validationResponse;
                }

                var result = await _unifiedService.UpdateTemplateAsync(templateId, request!);

                var statusCode = result.Success ? System.Net.HttpStatusCode.OK : 
                    (result.Message.Contains("no encontrado") ? System.Net.HttpStatusCode.NotFound : System.Net.HttpStatusCode.BadRequest);

                var successResponse = req.CreateResponse(statusCode);
                await successResponse.WriteAsJsonAsync(new ApiResponse<NotificationTemplate>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = (int)statusCode
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error actualizando template");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<NotificationTemplate>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("NotificationTemplates_DeleteFunction")]
        public async Task<HttpResponseData> DeleteAsync(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "notifications/templates/{templateId:int}")] HttpRequestData req,
            FunctionContext executionContext,
            int templateId)
        {
            var logger = executionContext.GetLogger("NotificationTemplates_DeleteFunction");
            
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

            logger.LogInformation($"Eliminando template de notificación: {templateId}");
            
            try
            {
                var result = await _unifiedService.DeleteTemplateAsync(templateId);

                var statusCode = result.Success ? System.Net.HttpStatusCode.OK : 
                    (result.Message.Contains("no encontrado") ? System.Net.HttpStatusCode.NotFound : System.Net.HttpStatusCode.BadRequest);

                var successResponse = req.CreateResponse(statusCode);
                await successResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = (int)statusCode
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error eliminando template");
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

        [Function("NotificationTemplates_GetByTypeFunction")]
        public async Task<HttpResponseData> GetByTypeAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notifications/templates/by-type/{notificationType}")] HttpRequestData req,
            FunctionContext executionContext,
            string notificationType)
        {
            var logger = executionContext.GetLogger("NotificationTemplates_GetByTypeFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<NotificationTemplate>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation($"Consultando templates por tipo: {notificationType}");
            
            try
            {
                var result = await _unifiedService.GetTemplatesByTypeAsync(notificationType);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<NotificationTemplate>>
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
                logger.LogError(ex, "Error consultando templates por tipo");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<NotificationTemplate>>
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
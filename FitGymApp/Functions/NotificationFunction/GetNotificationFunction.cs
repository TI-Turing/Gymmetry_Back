using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO;
using FitGymApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using FitGymApp.Utils;

namespace FitGymApp.Functions.NotificationFunction
{
    public class GetNotificationFunction
    {
        private readonly ILogger<GetNotificationFunction> _logger;
        private readonly INotificationService _service;

        public GetNotificationFunction(ILogger<GetNotificationFunction> logger, INotificationService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetNotificationByIdFunction")]
        public async Task<ApiResponse<Notification>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "notification/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<Notification>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando Notification por Id: {id}");
            try
            {
                var result = _service.GetNotificationById(id);
                if (!result.Success)
                {
                    return new ApiResponse<Notification>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<Notification>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar Notification por Id.");
                return new ApiResponse<Notification>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllNotificationsFunction")]
        public async Task<ApiResponse<IEnumerable<Notification>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "notifications")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<Notification>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los Notifications activos.");
            try
            {
                var result = _service.GetAllNotifications();
                return new ApiResponse<IEnumerable<Notification>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los Notifications.");
                return new ApiResponse<IEnumerable<Notification>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindNotificationsByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<Notification>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "notifications/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<Notification>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando Notifications por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<Notification>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindNotificationsByFields(filters);
                return new ApiResponse<IEnumerable<Notification>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar Notifications por filtros.");
                return new ApiResponse<IEnumerable<Notification>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }
    }
}

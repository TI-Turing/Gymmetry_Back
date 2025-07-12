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

namespace FitGymApp.Functions.NotificationOptionFunction
{
    public class GetNotificationOptionFunction
    {
        private readonly ILogger<GetNotificationOptionFunction> _logger;
        private readonly INotificationOptionService _service;

        public GetNotificationOptionFunction(ILogger<GetNotificationOptionFunction> logger, INotificationOptionService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetNotificationOptionByIdFunction")]
        public async Task<ApiResponse<NotificationOption>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "notificationoption/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<NotificationOption>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando NotificationOption por Id: {id}");
            try
            {
                var result = _service.GetNotificationOptionById(id);
                if (!result.Success)
                {
                    return new ApiResponse<NotificationOption>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<NotificationOption>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar NotificationOption por Id.");
                return new ApiResponse<NotificationOption>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllNotificationOptionsFunction")]
        public async Task<ApiResponse<IEnumerable<NotificationOption>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "notificationoptions")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<NotificationOption>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los NotificationOptions activos.");
            try
            {
                var result = _service.GetAllNotificationOptions();
                return new ApiResponse<IEnumerable<NotificationOption>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los NotificationOptions.");
                return new ApiResponse<IEnumerable<NotificationOption>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindNotificationOptionsByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<NotificationOption>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "notificationoptions/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<NotificationOption>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando NotificationOptions por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<NotificationOption>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindNotificationOptionsByFields(filters);
                return new ApiResponse<IEnumerable<NotificationOption>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar NotificationOptions por filtros.");
                return new ApiResponse<IEnumerable<NotificationOption>>
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

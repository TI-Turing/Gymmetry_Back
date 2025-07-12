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

namespace FitGymApp.Functions.UninstallOptionFunction
{
    public class GetUninstallOptionFunction
    {
        private readonly ILogger<GetUninstallOptionFunction> _logger;
        private readonly IUninstallOptionService _service;

        public GetUninstallOptionFunction(ILogger<GetUninstallOptionFunction> logger, IUninstallOptionService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetUninstallOptionByIdFunction")]
        public async Task<ApiResponse<UninstallOption>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "uninstalloption/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<UninstallOption>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando UninstallOption por Id: {id}");
            try
            {
                var result = _service.GetUninstallOptionById(id);
                if (!result.Success)
                {
                    return new ApiResponse<UninstallOption>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<UninstallOption>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar UninstallOption por Id.");
                return new ApiResponse<UninstallOption>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllUninstallOptionsFunction")]
        public async Task<ApiResponse<IEnumerable<UninstallOption>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "uninstalloptions")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<UninstallOption>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los UninstallOptions activos.");
            try
            {
                var result = _service.GetAllUninstallOptions();
                return new ApiResponse<IEnumerable<UninstallOption>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los UninstallOptions.");
                return new ApiResponse<IEnumerable<UninstallOption>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindUninstallOptionsByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<UninstallOption>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "uninstalloptions/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<UninstallOption>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando UninstallOptions por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<UninstallOption>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindUninstallOptionsByFields(filters);
                return new ApiResponse<IEnumerable<UninstallOption>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar UninstallOptions por filtros.");
                return new ApiResponse<IEnumerable<UninstallOption>>
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

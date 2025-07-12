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

namespace FitGymApp.Functions.SubModuleFunction
{
    public class GetSubModuleFunction
    {
        private readonly ILogger<GetSubModuleFunction> _logger;
        private readonly ISubModuleService _service;

        public GetSubModuleFunction(ILogger<GetSubModuleFunction> logger, ISubModuleService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetSubModuleByIdFunction")]
        public async Task<ApiResponse<SubModule>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "submodule/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<SubModule>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando SubModule por Id: {id}");
            try
            {
                var result = _service.GetSubModuleById(id);
                if (!result.Success)
                {
                    return new ApiResponse<SubModule>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<SubModule>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar SubModule por Id.");
                return new ApiResponse<SubModule>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllSubModulesFunction")]
        public async Task<ApiResponse<IEnumerable<SubModule>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "submodules")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<SubModule>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los SubModules activos.");
            try
            {
                var result = _service.GetAllSubModules();
                return new ApiResponse<IEnumerable<SubModule>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los SubModules.");
                return new ApiResponse<IEnumerable<SubModule>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindSubModulesByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<SubModule>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "submodules/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<SubModule>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando SubModules por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<SubModule>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindSubModulesByFields(filters);
                return new ApiResponse<IEnumerable<SubModule>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar SubModules por filtros.");
                return new ApiResponse<IEnumerable<SubModule>>
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

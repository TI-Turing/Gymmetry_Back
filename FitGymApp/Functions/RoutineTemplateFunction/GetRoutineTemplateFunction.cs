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

namespace FitGymApp.Functions.RoutineTemplateFunction
{
    public class GetRoutineTemplateFunction
    {
        private readonly ILogger<GetRoutineTemplateFunction> _logger;
        private readonly IRoutineTemplateService _service;

        public GetRoutineTemplateFunction(ILogger<GetRoutineTemplateFunction> logger, IRoutineTemplateService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetRoutineTemplateByIdFunction")]
        public async Task<ApiResponse<RoutineTemplate>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "routinetemplate/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<RoutineTemplate>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando RoutineTemplate por Id: {id}");
            try
            {
                var result = _service.GetRoutineTemplateById(id);
                if (!result.Success)
                {
                    return new ApiResponse<RoutineTemplate>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<RoutineTemplate>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar RoutineTemplate por Id.");
                return new ApiResponse<RoutineTemplate>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllRoutineTemplatesFunction")]
        public async Task<ApiResponse<IEnumerable<RoutineTemplate>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "routinetemplates")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<RoutineTemplate>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los RoutineTemplates activos.");
            try
            {
                var result = _service.GetAllRoutineTemplates();
                return new ApiResponse<IEnumerable<RoutineTemplate>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los RoutineTemplates.");
                return new ApiResponse<IEnumerable<RoutineTemplate>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindRoutineTemplatesByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<RoutineTemplate>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "routinetemplates/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<RoutineTemplate>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando RoutineTemplates por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<RoutineTemplate>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindRoutineTemplatesByFields(filters);
                return new ApiResponse<IEnumerable<RoutineTemplate>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar RoutineTemplates por filtros.");
                return new ApiResponse<IEnumerable<RoutineTemplate>>
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

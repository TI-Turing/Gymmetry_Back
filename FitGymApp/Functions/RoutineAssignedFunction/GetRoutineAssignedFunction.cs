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

namespace FitGymApp.Functions.RoutineAssignedFunction
{
    public class GetRoutineAssignedFunction
    {
        private readonly ILogger<GetRoutineAssignedFunction> _logger;
        private readonly IRoutineAssignedService _service;

        public GetRoutineAssignedFunction(ILogger<GetRoutineAssignedFunction> logger, IRoutineAssignedService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetRoutineAssignedByIdFunction")]
        public async Task<ApiResponse<RoutineAssigned>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "routineassigned/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<RoutineAssigned>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando RoutineAssigned por Id: {id}");
            try
            {
                var result = await _service.GetRoutineAssignedByIdAsync(id);
                if (!result.Success)
                {
                    return new ApiResponse<RoutineAssigned>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<RoutineAssigned>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar RoutineAssigned por Id.");
                return new ApiResponse<RoutineAssigned>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllRoutineAssignedsFunction")]
        public async Task<ApiResponse<IEnumerable<RoutineAssigned>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "routineassigneds")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<RoutineAssigned>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los RoutineAssigneds activos.");
            try
            {
                var result = await _service.GetAllRoutineAssignedsAsync();
                return new ApiResponse<IEnumerable<RoutineAssigned>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los RoutineAssigneds.");
                return new ApiResponse<IEnumerable<RoutineAssigned>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindRoutineAssignedsByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<RoutineAssigned>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "routineassigneds/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<RoutineAssigned>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando RoutineAssigneds por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<RoutineAssigned>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = await _service.FindRoutineAssignedsByFieldsAsync(filters);
                return new ApiResponse<IEnumerable<RoutineAssigned>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar RoutineAssigneds por filtros.");
                return new ApiResponse<IEnumerable<RoutineAssigned>>
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

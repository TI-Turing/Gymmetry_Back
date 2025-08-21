using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;
using Gymmetry.Utils;
using System.Net;
using Newtonsoft.Json;

namespace Gymmetry.Functions.RoutineAssignedFunction
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

        private static JsonSerializerOptions JsonCycleOptions => new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };

        [Function("RoutineAssigned_GetRoutineAssignedFunction")]
        public async Task<HttpResponseData> GetByIdAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "routineassigned/{id:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid id)
        {
            var logger = executionContext.GetLogger("RoutineAssigned_GetRoutineAssignedFunction");
            logger.LogInformation($"Consultando RoutineAssigned por Id: {id}");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<RoutineAssigned>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            try
            {
                var result = await _service.GetRoutineAssignedByIdAsync(id);
                if (!result.Success)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new ApiResponse<RoutineAssigned>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                    return notFoundResponse;
                }
                // Evitar ciclos explicitamente eliminando colecciones recursivas pesadas si existen
                if (result.Data?.RoutineTemplate != null)
                {
                    // Rompe posibles ciclos quitando la colección inversa
                    result.Data.RoutineTemplate.RoutineAssigneds = null!; // será ignorado por IgnoreCycles
                }
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                var payload = new ApiResponse<RoutineAssigned>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
                await successResponse.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(payload, JsonCycleOptions));
                successResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar RoutineAssigned por Id.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<RoutineAssigned>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("RoutineAssigned_GetAllRoutineAssignedsFunction")]
        public async Task<HttpResponseData> GetAllAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "routineassigneds")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("RoutineAssigned_GetAllRoutineAssignedsFunction");
            logger.LogInformation("Consultando todos los RoutineAssigneds activos.");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<RoutineAssigned>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            try
            {
                var result = await _service.GetAllRoutineAssignedsAsync();
                // Romper ciclos en cada elemento
                foreach (var ra in result.Data ?? Enumerable.Empty<RoutineAssigned>())
                {
                    ra.RoutineTemplate?.RoutineAssigneds?.Clear();
                }
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                var payload = new ApiResponse<IEnumerable<RoutineAssigned>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
                await successResponse.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(payload, JsonCycleOptions));
                successResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar todos los RoutineAssigneds.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<RoutineAssigned>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("RoutineAssigned_FindRoutineAssignedsByFieldsFunction")]
        public async Task<HttpResponseData> FindByFieldsAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "routineassigneds/find")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("RoutineAssigned_FindRoutineAssignedsByFieldsFunction");
            logger.LogInformation("Consultando RoutineAssigneds por filtros dinámicos.");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<RoutineAssigned>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<RoutineAssigned>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badResponse;
                }
                var result = await _service.FindRoutineAssignedsByFieldsAsync(filters);
                foreach (var ra in result.Data ?? Enumerable.Empty<RoutineAssigned>())
                {
                    ra.RoutineTemplate?.RoutineAssigneds?.Clear();
                }
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                var payload = new ApiResponse<IEnumerable<RoutineAssigned>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
                await successResponse.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(payload, JsonCycleOptions));
                successResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar RoutineAssigneds por filtros.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<RoutineAssigned>>
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

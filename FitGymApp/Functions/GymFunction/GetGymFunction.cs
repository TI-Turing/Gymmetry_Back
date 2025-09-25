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
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using Gymmetry.Utils;
using Microsoft.AspNetCore.Http;

namespace Gymmetry.Functions.GymFunction
{
    public class GetGymFunction
    {
        private readonly ILogger<GetGymFunction> _logger;
        private readonly IGymService _service;

        public GetGymFunction(ILogger<GetGymFunction> logger, IGymService service)
        {
            _logger = logger;
            _service = service;
        }

        private static void AddCors(HttpRequestData req, HttpResponseData resp)
        {
            string? origin = req.Headers.TryGetValues("Origin", out var ov) ? ov.FirstOrDefault() : null;
            string? reqHeaders = req.Headers.TryGetValues("Access-Control-Request-Headers", out var rh) ? rh.FirstOrDefault() : null;
            CorsHelper.AddCorsHeaders(resp, origin, reqHeaders);
        }

        [Function("Gym_GetGymByIdFunction")]
        public async Task<HttpResponseData> GetByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "gym/{id:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid id)
        {
            var logger = executionContext.GetLogger("Gym_GetGymByIdFunction");
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var pre = req.CreateResponse(HttpStatusCode.NoContent);
                AddCors(req, pre);
                return pre;
            }
            logger.LogInformation($"Consultando Gym por Id: {id}");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                AddCors(req, unauthorizedResponse);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<Guid>
                {
                    Success = false,
                    Message = error!,
                    Data = default,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            try
            {
                var result = await _service.GetGymByIdAsync(id);
                var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.NotFound);
                AddCors(req, response);
                await response.WriteAsJsonAsync(new ApiResponse<Gym>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status404NotFound
                });
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar Gym por Id.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                AddCors(req, errorResponse);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<Gym>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("Gym_GetAllGymsFunction")]
        public async Task<HttpResponseData> GetAllAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "gyms")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Gym_GetAllGymsFunction");
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var pre = req.CreateResponse(HttpStatusCode.NoContent);
                AddCors(req, pre);
                return pre;
            }
            logger.LogInformation("Consultando todos los Gyms activos.");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                AddCors(req, unauthorizedResponse);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<Guid>
                {
                    Success = false,
                    Message = error!,
                    Data = default,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            try
            {
                var result = await _service.GetAllGymsAsync();
                var response = req.CreateResponse(HttpStatusCode.OK);
                AddCors(req, response);
                await response.WriteAsJsonAsync(new ApiResponse<IEnumerable<Gym>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar todos los Gyms.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                AddCors(req, errorResponse);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Gym>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("Gym_FindGymsByFieldsFunction")]
        public async Task<HttpResponseData> FindByFieldsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "gyms/find")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Gym_FindGymsByFieldsFunction");
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var pre = req.CreateResponse(HttpStatusCode.NoContent);
                AddCors(req, pre);
                return pre;
            }
            logger.LogInformation("Consultando Gyms por filtros dinámicos.");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                AddCors(req, unauthorizedResponse);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<Guid>
                {
                    Success = false,
                    Message = error!,
                    Data = default,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    AddCors(req, badResponse);
                    await badResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Gym>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badResponse;
                }
                var result = await _service.FindGymsByFieldsAsync(filters);
                var response = req.CreateResponse(HttpStatusCode.OK);
                AddCors(req, response);
                await response.WriteAsJsonAsync(new ApiResponse<IEnumerable<Gym>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar Gyms por filtros.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                AddCors(req, errorResponse);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Gym>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("Gym_FindGymsByNameFunction")]
        public async Task<HttpResponseData> FindByNameAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "options", Route = "gyms/findbyname/{name}")] HttpRequestData req,
            FunctionContext executionContext,
            string name)
        {
            var logger = executionContext.GetLogger("Gym_FindGymsByNameFunction");
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var pre = req.CreateResponse(HttpStatusCode.NoContent);
                AddCors(req, pre);
                return pre;
            }
            logger.LogInformation($"Consultando Gyms por nombre: {name}");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                AddCors(req, unauthorizedResponse);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<Guid>
                {
                    Success = false,
                    Message = error!,
                    Data = default,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            try
            {
                var result = await _service.FindGymsByNameAsync(name);
                var response = req.CreateResponse(HttpStatusCode.OK);
                AddCors(req, response);
                await response.WriteAsJsonAsync(new ApiResponse<IEnumerable<Gym>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar Gyms por nombre.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                AddCors(req, errorResponse);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Gym>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
                return errorResponse;
            }
        }
    }
}

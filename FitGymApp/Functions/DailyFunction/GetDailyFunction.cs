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
using System.Threading.Tasks;
using System.Linq;
using Gymmetry.Utils;

namespace Gymmetry.Functions.DailyFunction
{
    public class GetDailyFunction
    {
        private readonly ILogger<GetDailyFunction> _logger;
        private readonly IDailyService _service;

        public GetDailyFunction(ILogger<GetDailyFunction> logger, IDailyService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("Daily_GetDailyByIdFunction")]
        public async Task<HttpResponseData> GetByIdAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "daily/{id:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid id)
        {
            var logger = executionContext.GetLogger("Daily_GetDailyByIdFunction");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<Daily>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            logger.LogInformation($"Consultando Daily por Id: {id}");
            try
            {
                var result = await _service.GetDailyByIdAsync(id);
                if (!result.Success)
                {
                    var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new ApiResponse<Daily>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                    return notFoundResponse;
                }
                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<Daily>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar Daily por Id.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<Daily>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("Daily_GetAllDailiesFunction")]
        public async Task<HttpResponseData> GetAllAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "dailies")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Daily_GetAllDailiesFunction");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Daily>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            logger.LogInformation("Consultando todos los Dailies activos.");
            try
            {
                var result = await _service.GetAllDailiesAsync();
                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Daily>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar todos los Dailies.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Daily>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("Daily_FindDailiesByFieldsFunction")]
        public async Task<HttpResponseData> FindByFieldsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "dailies/find")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Daily_FindDailiesByFieldsFunction");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Daily>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            logger.LogInformation("Consultando Dailies por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Daily>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badResponse;
                }
                var result = await _service.FindDailiesByFieldsAsync(filters);
                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Daily>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar Dailies por filtros.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<Daily>>
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

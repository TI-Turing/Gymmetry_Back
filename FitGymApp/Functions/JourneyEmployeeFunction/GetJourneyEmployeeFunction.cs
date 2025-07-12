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

namespace FitGymApp.Functions.JourneyEmployeeFunction
{
    public class GetJourneyEmployeeFunction
    {
        private readonly ILogger<GetJourneyEmployeeFunction> _logger;
        private readonly IJourneyEmployeeService _service;

        public GetJourneyEmployeeFunction(ILogger<GetJourneyEmployeeFunction> logger, IJourneyEmployeeService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetJourneyEmployeeByIdFunction")]
        public async Task<ApiResponse<JourneyEmployee>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "journeyemployee/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<JourneyEmployee>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando JourneyEmployee por Id: {id}");
            try
            {
                var result = _service.GetJourneyEmployeeById(id);
                if (!result.Success)
                {
                    return new ApiResponse<JourneyEmployee>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<JourneyEmployee>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar JourneyEmployee por Id.");
                return new ApiResponse<JourneyEmployee>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllJourneyEmployeesFunction")]
        public async Task<ApiResponse<IEnumerable<JourneyEmployee>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "journeyemployees")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<JourneyEmployee>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los JourneyEmployees activos.");
            try
            {
                var result = _service.GetAllJourneyEmployees();
                return new ApiResponse<IEnumerable<JourneyEmployee>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los JourneyEmployees.");
                return new ApiResponse<IEnumerable<JourneyEmployee>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindJourneyEmployeesByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<JourneyEmployee>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "journeyemployees/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<JourneyEmployee>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando JourneyEmployees por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<JourneyEmployee>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindJourneyEmployeesByFields(filters);
                return new ApiResponse<IEnumerable<JourneyEmployee>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar JourneyEmployees por filtros.");
                return new ApiResponse<IEnumerable<JourneyEmployee>>
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

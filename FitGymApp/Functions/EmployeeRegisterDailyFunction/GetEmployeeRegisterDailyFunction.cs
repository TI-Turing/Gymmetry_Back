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

namespace FitGymApp.Functions.EmployeeRegisterDailyFunction
{
    public class GetEmployeeRegisterDailyFunction
    {
        private readonly ILogger<GetEmployeeRegisterDailyFunction> _logger;
        private readonly IEmployeeRegisterDailyService _service;

        public GetEmployeeRegisterDailyFunction(ILogger<GetEmployeeRegisterDailyFunction> logger, IEmployeeRegisterDailyService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetEmployeeRegisterDailyByIdFunction")]
        public async Task<ApiResponse<EmployeeRegisterDaily>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "employeeregisterdaily/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<EmployeeRegisterDaily>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando EmployeeRegisterDaily por Id: {id}");
            try
            {
                var result = _service.GetEmployeeRegisterDailyById(id);
                if (!result.Success)
                {
                    return new ApiResponse<EmployeeRegisterDaily>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<EmployeeRegisterDaily>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar EmployeeRegisterDaily por Id.");
                return new ApiResponse<EmployeeRegisterDaily>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllEmployeeRegisterDailiesFunction")]
        public async Task<ApiResponse<IEnumerable<EmployeeRegisterDaily>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "employeeregisterdailies")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<EmployeeRegisterDaily>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los EmployeeRegisterDailies activos.");
            try
            {
                var result = _service.GetAllEmployeeRegisterDailies();
                return new ApiResponse<IEnumerable<EmployeeRegisterDaily>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los EmployeeRegisterDailies.");
                return new ApiResponse<IEnumerable<EmployeeRegisterDaily>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindEmployeeRegisterDailiesByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<EmployeeRegisterDaily>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "employeeregisterdailies/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<EmployeeRegisterDaily>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando EmployeeRegisterDailies por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<EmployeeRegisterDaily>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindEmployeeRegisterDailiesByFields(filters);
                return new ApiResponse<IEnumerable<EmployeeRegisterDaily>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar EmployeeRegisterDailies por filtros.");
                return new ApiResponse<IEnumerable<EmployeeRegisterDaily>>
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

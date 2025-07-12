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

namespace FitGymApp.Functions.PlanFunction
{
    public class GetPlanFunction
    {
        private readonly ILogger<GetPlanFunction> _logger;
        private readonly IPlanService _service;

        public GetPlanFunction(ILogger<GetPlanFunction> logger, IPlanService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetPlanByIdFunction")]
        public async Task<ApiResponse<Plan>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "plan/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<Plan>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando Plan por Id: {id}");
            try
            {
                var result = _service.GetPlanById(id);
                if (!result.Success)
                {
                    return new ApiResponse<Plan>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<Plan>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar Plan por Id.");
                return new ApiResponse<Plan>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllPlansFunction")]
        public async Task<ApiResponse<IEnumerable<Plan>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "plans")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<Plan>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los Plans activos.");
            try
            {
                var result = _service.GetAllPlans();
                return new ApiResponse<IEnumerable<Plan>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los Plans.");
                return new ApiResponse<IEnumerable<Plan>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindPlansByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<Plan>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "plans/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<Plan>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando Plans por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<Plan>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindPlansByFields(filters);
                return new ApiResponse<IEnumerable<Plan>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar Plans por filtros.");
                return new ApiResponse<IEnumerable<Plan>>
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

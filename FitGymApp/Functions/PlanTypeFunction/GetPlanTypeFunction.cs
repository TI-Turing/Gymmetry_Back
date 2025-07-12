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

namespace FitGymApp.Functions.PlanTypeFunction
{
    public class GetPlanTypeFunction
    {
        private readonly ILogger<GetPlanTypeFunction> _logger;
        private readonly IPlanTypeService _service;

        public GetPlanTypeFunction(ILogger<GetPlanTypeFunction> logger, IPlanTypeService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetPlanTypeByIdFunction")]
        public async Task<ApiResponse<PlanType>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "plantype/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<PlanType>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando PlanType por Id: {id}");
            try
            {
                var result = _service.GetPlanTypeById(id);
                if (!result.Success)
                {
                    return new ApiResponse<PlanType>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<PlanType>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar PlanType por Id.");
                return new ApiResponse<PlanType>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllPlanTypesFunction")]
        public async Task<ApiResponse<IEnumerable<PlanType>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "plantypes")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<PlanType>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los PlanTypes activos.");
            try
            {
                var result = _service.GetAllPlanTypes();
                return new ApiResponse<IEnumerable<PlanType>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los PlanTypes.");
                return new ApiResponse<IEnumerable<PlanType>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindPlanTypesByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<PlanType>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "plantypes/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<PlanType>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando PlanTypes por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<PlanType>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindPlanTypesByFields(filters);
                return new ApiResponse<IEnumerable<PlanType>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar PlanTypes por filtros.");
                return new ApiResponse<IEnumerable<PlanType>>
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

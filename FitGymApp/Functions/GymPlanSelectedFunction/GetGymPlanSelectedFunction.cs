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

namespace FitGymApp.Functions.GymPlanSelectedFunction
{
    public class GetGymPlanSelectedFunction
    {
        private readonly ILogger<GetGymPlanSelectedFunction> _logger;
        private readonly IGymPlanSelectedService _service;

        public GetGymPlanSelectedFunction(ILogger<GetGymPlanSelectedFunction> logger, IGymPlanSelectedService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetGymPlanSelectedByIdFunction")]
        public async Task<ApiResponse<GymPlanSelected>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "gymplanselected/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<GymPlanSelected>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando GymPlanSelected por Id: {id}");
            try
            {
                var result = _service.GetGymPlanSelectedById(id);
                if (!result.Success)
                {
                    return new ApiResponse<GymPlanSelected>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<GymPlanSelected>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar GymPlanSelected por Id.");
                return new ApiResponse<GymPlanSelected>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllGymPlanSelectedsFunction")]
        public async Task<ApiResponse<IEnumerable<GymPlanSelected>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "gymplanselecteds")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<GymPlanSelected>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los GymPlanSelecteds activos.");
            try
            {
                var result = _service.GetAllGymPlanSelecteds();
                return new ApiResponse<IEnumerable<GymPlanSelected>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los GymPlanSelecteds.");
                return new ApiResponse<IEnumerable<GymPlanSelected>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindGymPlanSelectedsByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<GymPlanSelected>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "gymplanselecteds/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<GymPlanSelected>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando GymPlanSelecteds por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<GymPlanSelected>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindGymPlanSelectedsByFields(filters);
                return new ApiResponse<IEnumerable<GymPlanSelected>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar GymPlanSelecteds por filtros.");
                return new ApiResponse<IEnumerable<GymPlanSelected>>
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

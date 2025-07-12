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

namespace FitGymApp.Functions.PhysicalAssessmentFunction
{
    public class GetPhysicalAssessmentFunction
    {
        private readonly ILogger<GetPhysicalAssessmentFunction> _logger;
        private readonly IPhysicalAssessmentService _service;

        public GetPhysicalAssessmentFunction(ILogger<GetPhysicalAssessmentFunction> logger, IPhysicalAssessmentService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetPhysicalAssessmentByIdFunction")]
        public async Task<ApiResponse<PhysicalAssessment>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "physicalassessment/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<PhysicalAssessment>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando PhysicalAssessment por Id: {id}");
            try
            {
                var result = _service.GetPhysicalAssessmentById(id);
                if (!result.Success)
                {
                    return new ApiResponse<PhysicalAssessment>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<PhysicalAssessment>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar PhysicalAssessment por Id.");
                return new ApiResponse<PhysicalAssessment>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllPhysicalAssessmentsFunction")]
        public async Task<ApiResponse<IEnumerable<PhysicalAssessment>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "physicalassessments")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<PhysicalAssessment>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los PhysicalAssessments activos.");
            try
            {
                var result = _service.GetAllPhysicalAssessments();
                return new ApiResponse<IEnumerable<PhysicalAssessment>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los PhysicalAssessments.");
                return new ApiResponse<IEnumerable<PhysicalAssessment>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindPhysicalAssessmentsByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<PhysicalAssessment>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "physicalassessments/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<PhysicalAssessment>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando PhysicalAssessments por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<PhysicalAssessment>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindPhysicalAssessmentsByFields(filters);
                return new ApiResponse<IEnumerable<PhysicalAssessment>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar PhysicalAssessments por filtros.");
                return new ApiResponse<IEnumerable<PhysicalAssessment>>
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

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

namespace FitGymApp.Functions.ScheduleFunction
{
    public class GetScheduleFunction
    {
        private readonly ILogger<GetScheduleFunction> _logger;
        private readonly IScheduleService _service;

        public GetScheduleFunction(ILogger<GetScheduleFunction> logger, IScheduleService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetScheduleByIdFunction")]
        public async Task<ApiResponse<Schedule>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedule/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<Schedule>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando Schedule por Id: {id}");
            try
            {
                var result = _service.GetScheduleById(id);
                if (!result.Success)
                {
                    return new ApiResponse<Schedule>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<Schedule>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar Schedule por Id.");
                return new ApiResponse<Schedule>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllSchedulesFunction")]
        public async Task<ApiResponse<IEnumerable<Schedule>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "schedules")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<Schedule>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los Schedules activos.");
            try
            {
                var result = _service.GetAllSchedules();
                return new ApiResponse<IEnumerable<Schedule>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los Schedules.");
                return new ApiResponse<IEnumerable<Schedule>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindSchedulesByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<Schedule>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "schedules/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<Schedule>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando Schedules por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<Schedule>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindSchedulesByFields(filters);
                return new ApiResponse<IEnumerable<Schedule>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar Schedules por filtros.");
                return new ApiResponse<IEnumerable<Schedule>>
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

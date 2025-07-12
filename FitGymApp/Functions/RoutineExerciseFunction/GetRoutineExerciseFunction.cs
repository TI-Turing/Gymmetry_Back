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

namespace FitGymApp.Functions.RoutineExerciseFunction
{
    public class GetRoutineExerciseFunction
    {
        private readonly ILogger<GetRoutineExerciseFunction> _logger;
        private readonly IRoutineExerciseService _service;

        public GetRoutineExerciseFunction(ILogger<GetRoutineExerciseFunction> logger, IRoutineExerciseService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetRoutineExerciseByIdFunction")]
        public async Task<ApiResponse<RoutineExercise>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "routineexercise/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<RoutineExercise>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando RoutineExercise por Id: {id}");
            try
            {
                var result = _service.GetRoutineExerciseById(id);
                if (!result.Success)
                {
                    return new ApiResponse<RoutineExercise>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<RoutineExercise>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar RoutineExercise por Id.");
                return new ApiResponse<RoutineExercise>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllRoutineExercisesFunction")]
        public async Task<ApiResponse<IEnumerable<RoutineExercise>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "routineexercises")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<RoutineExercise>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los RoutineExercises activos.");
            try
            {
                var result = _service.GetAllRoutineExercises();
                return new ApiResponse<IEnumerable<RoutineExercise>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los RoutineExercises.");
                return new ApiResponse<IEnumerable<RoutineExercise>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindRoutineExercisesByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<RoutineExercise>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "routineexercises/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<RoutineExercise>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando RoutineExercises por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<RoutineExercise>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindRoutineExercisesByFields(filters);
                return new ApiResponse<IEnumerable<RoutineExercise>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar RoutineExercises por filtros.");
                return new ApiResponse<IEnumerable<RoutineExercise>>
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

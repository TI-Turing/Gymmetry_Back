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

namespace FitGymApp.Functions.CategoryExerciseFunction
{
    public class GetCategoryExerciseFunction
    {
        private readonly ILogger<GetCategoryExerciseFunction> _logger;
        private readonly ICategoryExerciseService _service;

        public GetCategoryExerciseFunction(ILogger<GetCategoryExerciseFunction> logger, ICategoryExerciseService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("CategoryExercise_GetCategoryExerciseByIdFunction")]
        public async Task<ApiResponse<CategoryExercise>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "categoryexercise/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                return new ApiResponse<CategoryExercise>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando CategoryExercise por Id: {id}");
            try
            {
                var result = await _service.GetCategoryExerciseByIdAsync(id);
                if (!result.Success)
                {
                    return new ApiResponse<CategoryExercise>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<CategoryExercise>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar CategoryExercise por Id.");
                return new ApiResponse<CategoryExercise>
                {
                    Success = false,
                    Message = "Ocurri� un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("CategoryExercise_GetAllCategoryExercisesFunction")]
        public async Task<ApiResponse<IEnumerable<CategoryExercise>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "categoryexercises")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                return new ApiResponse<IEnumerable<CategoryExercise>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los CategoryExercises activos.");
            try
            {
                var result = await _service.GetAllCategoryExercisesAsync();
                return new ApiResponse<IEnumerable<CategoryExercise>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los CategoryExercises.");
                return new ApiResponse<IEnumerable<CategoryExercise>>
                {
                    Success = false,
                    Message = "Ocurri� un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("CategoryExercise_FindCategoryExercisesByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<CategoryExercise>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "categoryexercises/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                return new ApiResponse<IEnumerable<CategoryExercise>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando CategoryExercises por filtros din�micos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<CategoryExercise>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros v�lidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = await _service.FindCategoryExercisesByFieldsAsync(filters);
                return new ApiResponse<IEnumerable<CategoryExercise>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar CategoryExercises por filtros.");
                return new ApiResponse<IEnumerable<CategoryExercise>>
                {
                    Success = false,
                    Message = "Ocurri� un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }
    }
}

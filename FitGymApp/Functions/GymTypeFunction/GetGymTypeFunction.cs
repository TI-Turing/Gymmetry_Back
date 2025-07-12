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

namespace FitGymApp.Functions.GymTypeFunction
{
    public class GetGymTypeFunction
    {
        private readonly ILogger<GetGymTypeFunction> _logger;
        private readonly IGymTypeService _service;

        public GetGymTypeFunction(ILogger<GetGymTypeFunction> logger, IGymTypeService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GetGymTypeByIdFunction")]
        public async Task<ApiResponse<GymType>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "gymtype/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<GymType>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando GymType por Id: {id}");
            try
            {
                var result = _service.GetGymTypeById(id);
                if (!result.Success)
                {
                    return new ApiResponse<GymType>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<GymType>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar GymType por Id.");
                return new ApiResponse<GymType>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("GetAllGymTypesFunction")]
        public async Task<ApiResponse<IEnumerable<GymType>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "gymtypes")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<GymType>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todos los GymTypes activos.");
            try
            {
                var result = _service.GetAllGymTypes();
                return new ApiResponse<IEnumerable<GymType>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todos los GymTypes.");
                return new ApiResponse<IEnumerable<GymType>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("FindGymTypesByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<GymType>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "gymtypes/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<GymType>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando GymTypes por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<GymType>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _service.FindGymTypesByFields(filters);
                return new ApiResponse<IEnumerable<GymType>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar GymTypes por filtros.");
                return new ApiResponse<IEnumerable<GymType>>
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

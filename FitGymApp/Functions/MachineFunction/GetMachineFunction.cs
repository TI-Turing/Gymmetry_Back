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
using FitGymApp.Utils;

namespace FitGymApp.Functions.MachineFunction
{
    public class GetMachineFunction
    {
        private readonly ILogger<GetMachineFunction> _logger;
        private readonly IMachineService _service;

        public GetMachineFunction(ILogger<GetMachineFunction> logger, IMachineService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("Machine_GetMachineByIdFunction")]
        public async Task<ApiResponse<Machine>> GetByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "machine/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<Machine>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation($"Consultando Machine por Id: {id}");
            try
            {
                var result = await _service.GetMachineByIdAsync(id);
                if (!result.Success)
                {
                    return new ApiResponse<Machine>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<Machine>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar Machine por Id.");
                return new ApiResponse<Machine>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("Machine_GetAllMachinesFunction")]
        public async Task<ApiResponse<IEnumerable<Machine>>> GetAllAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "machines")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<Machine>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando todas las Machines activas.");
            try
            {
                var result = await _service.GetAllMachinesAsync();
                return new ApiResponse<IEnumerable<Machine>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar todas las Machines.");
                return new ApiResponse<IEnumerable<Machine>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }

        [Function("Machine_FindMachinesByFieldsFunction")]
        public async Task<ApiResponse<IEnumerable<Machine>>> FindByFieldsAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "machines/find")] HttpRequest req)
        {
            if (!JwtValidator.ValidateJwt(req, out var error))
            {
                return new ApiResponse<IEnumerable<Machine>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
            _logger.LogInformation("Consultando Machines por filtros dinámicos.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    return new ApiResponse<IEnumerable<Machine>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = await _service.FindMachinesByFieldsAsync(filters);
                return new ApiResponse<IEnumerable<Machine>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar Machines por filtros.");
                return new ApiResponse<IEnumerable<Machine>>
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

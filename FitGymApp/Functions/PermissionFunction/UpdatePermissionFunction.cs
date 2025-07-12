using FitGymApp.Domain.DTO.Permission.Request;
using FitGymApp.Domain.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using FitGymApp.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FitGymApp.Utils;
namespace FitGymApp.Functions.PermissionFunction;
public class UpdatePermissionFunction
{
    private readonly ILogger<UpdatePermissionFunction> _logger;
    private readonly IPermissionService _service;
    public UpdatePermissionFunction(ILogger<UpdatePermissionFunction> logger, IPermissionService service)
    {
        _logger = logger;
        _service = service;
    }
    [Function("Permission_UpdatePermissionFunction")]
    public async Task<ApiResponse<Guid>> UpdateAsync([HttpTrigger(AuthorizationLevel.Function, "put", Route = "permission/update")] HttpRequest req)
    {
        if (!JwtValidator.ValidateJwt(req, out var error))
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = error!,
                Data = default,
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }
        _logger.LogInformation("Procesando solicitud para actualizar un Permission.");
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var objRequest = JsonConvert.DeserializeObject<UpdatePermissionRequest>(requestBody);
            var validationResult = ModelValidator.ValidateModel<UpdatePermissionRequest, Guid>(objRequest, StatusCodes.Status400BadRequest);
            if (validationResult is not null) return validationResult;
            var result = await _service.UpdatePermissionAsync(objRequest);
            if (!result.Success)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = result.Message,
                    Data = objRequest.Id,
                    StatusCode = StatusCodes.Status404NotFound
                };
            }
            return new ApiResponse<Guid>
            {
                Success = true,
                Message = result.Message,
                Data = objRequest.Id,
                StatusCode = StatusCodes.Status200OK
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar Permission.");
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "Ocurrió un error al procesar la solicitud.",
                Data = default,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }
}
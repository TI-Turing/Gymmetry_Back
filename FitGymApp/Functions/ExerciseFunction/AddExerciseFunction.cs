using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Gymmetry.Domain.DTO.Exercise.Request;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Gymmetry.Domain.DTO;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Gymmetry.Utils;

namespace Gymmetry.Functions.ExerciseFunction;

public class AddExerciseFunction
{
    private readonly ILogger<AddExerciseFunction> _logger;
    private readonly IExerciseService _service;

    public AddExerciseFunction(ILogger<AddExerciseFunction> logger, IExerciseService service)
    {
        _logger = logger;
        _service = service;
    }

    [Function("Exercise_AddExerciseFunction")]
    public async Task<ApiResponse<Guid>> AddAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "exercise/add")] HttpRequest req)
    {
        if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = error!,
                Data = default,
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        _logger.LogInformation("Procesando solicitud para agregar un Exercise.");
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var objRequest = JsonConvert.DeserializeObject<AddExerciseRequest>(requestBody);
            var validationResult = ModelValidator.ValidateModel<AddExerciseRequest, Guid>(objRequest, StatusCodes.Status400BadRequest);
            if (validationResult is not null) return validationResult;

            var result = await _service.CreateExerciseAsync(objRequest);
            if (!result.Success)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = result.Message,
                    Data = default,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            return new ApiResponse<Guid>
            {
                Success = true,
                Message = result.Message,
                Data = result.Data != null ? result.Data.Id : default,
                StatusCode = StatusCodes.Status200OK
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar Exercise.");
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "Ocurri� un error al procesar la solicitud.",
                Data = default,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }
}

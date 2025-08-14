using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Gymmetry.Domain.DTO.DailyExercise.Request;
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
using Microsoft.Azure.Functions.Worker.Http;

namespace Gymmetry.Functions.DailyExerciseFunction;

public class AddDailyExerciseFunction
{
    private readonly ILogger<AddDailyExerciseFunction> _logger;
    private readonly IDailyExerciseService _service;

    public AddDailyExerciseFunction(ILogger<AddDailyExerciseFunction> logger, IDailyExerciseService service)
    {
        _logger = logger;
        _service = service;
    }

    [Function("DailyExercise_AddDailyExerciseFunction")]
    public async Task<HttpResponseData> AddAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dailyexercise/add")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("DailyExercise_AddDailyExerciseFunction");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var objRequest = JsonConvert.DeserializeObject<AddDailyExerciseRequest>(requestBody);
        FunctionResponseHelper.LogRequest(logger, "DailyExercise_AddDailyExerciseFunction", objRequest);
        if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
        {
            return await FunctionResponseHelper.CreateResponseAsync(req, System.Net.HttpStatusCode.Unauthorized,
                new ApiResponse<Guid>
                {
                    Success = false,
                    Message = error!,
                    Data = default,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
        }
        try
        {
            var validationResult = ModelValidator.ValidateModel<AddDailyExerciseRequest, Guid>(objRequest, StatusCodes.Status400BadRequest);
            if (validationResult is not null)
            {
                return await FunctionResponseHelper.CreateResponseAsync(req, System.Net.HttpStatusCode.BadRequest, validationResult);
            }
            var ip = FunctionResponseHelper.GetClientIp(req);
            objRequest.Ip = ip;
            var result = await _service.CreateDailyExerciseAsync(objRequest);
            if (!result.Success)
            {
                return await FunctionResponseHelper.CreateResponseAsync(req, System.Net.HttpStatusCode.BadRequest,
                    new ApiResponse<Guid>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = default,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
            }
            return await FunctionResponseHelper.CreateResponseAsync(req, System.Net.HttpStatusCode.OK,
                new ApiResponse<Guid>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data != null ? result.Data.Id : default,
                    StatusCode = StatusCodes.Status200OK
                });
        }
        catch (Exception ex)
        {
            FunctionResponseHelper.LogError(logger, "DailyExercise_AddDailyExerciseFunction", ex);
            return await FunctionResponseHelper.CreateResponseAsync(req, System.Net.HttpStatusCode.BadRequest,
                new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = default,
                    StatusCode = StatusCodes.Status400BadRequest
                });
        }
    }

    [Function("DailyExercise_AddDailyExercisesBulkFunction")]
    public async Task<ApiResponse<IEnumerable<Guid>>> AddBulkAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "dailyexercises/addbulk")] HttpRequest req)
    {
        if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
        {
            return new ApiResponse<IEnumerable<Guid>>
            {
                Success = false,
                Message = error!,
                Data = null,
                StatusCode = StatusCodes.Status401Unauthorized
            };
        }

        _logger.LogInformation("Procesando solicitud para agregar ejercicios diarios masivamente.");
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var objRequest = JsonConvert.DeserializeObject<List<AddDailyExerciseRequest>>(requestBody);
            if (objRequest == null || !objRequest.Any())
            {
                return new ApiResponse<IEnumerable<Guid>>
                {
                    Success = false,
                    Message = "No se proporcionaron ejercicios válidos.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            var result = await _service.CreateDailyExercisesBulkAsync(objRequest);
            if (!result.Success)
            {
                return new ApiResponse<IEnumerable<Guid>>
                {
                    Success = false,
                    Message = result.Message,
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            return new ApiResponse<IEnumerable<Guid>>
            {
                Success = true,
                Message = result.Message,
                Data = result.Data?.Select(x => x.Id),
                StatusCode = StatusCodes.Status200OK
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar ejercicios diarios masivamente.");
            return new ApiResponse<IEnumerable<Guid>>
            {
                Success = false,
                Message = "Ocurrió un error al procesar la solicitud.",
                Data = null,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }
}

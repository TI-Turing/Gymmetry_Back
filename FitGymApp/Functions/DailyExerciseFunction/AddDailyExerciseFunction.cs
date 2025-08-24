using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.DailyExercise.Request;
using Gymmetry.Domain.Models;
using Gymmetry.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
    public async Task<HttpResponseData> AddBulkAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dailyexercises/addbulk")] HttpRequestData req,
            FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("DailyExercise_AddDailyExercisesBulkFunction");
        if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
        {
            return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.Unauthorized, new ApiResponse<IEnumerable<Guid>>
            {
                Success = false,
                Message = error!,
                Data = null,
                StatusCode = StatusCodes.Status401Unauthorized
            });
        }
        logger.LogInformation("Procesando solicitud para agregar ejercicios diarios masivamente.");
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var objRequest = JsonConvert.DeserializeObject<List<AddDailyExerciseRequest>>(requestBody);
            if (objRequest == null || !objRequest.Any())
            {
                return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<IEnumerable<Guid>>
                {
                    Success = false,
                    Message = "No se proporcionaron ejercicios válidos.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            var ip = FunctionResponseHelper.GetClientIp(req);
            foreach (var item in objRequest) item.Ip = ip;
            var result = await _service.CreateDailyExercisesBulkAsync(objRequest);
            if (!result.Success)
            {
                return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<IEnumerable<Guid>>
                {
                    Success = false,
                    Message = result.Message,
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
            return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.OK, new ApiResponse<IEnumerable<Guid>>
            {
                Success = true,
                Message = result.Message,
                Data = result.Data?.Select(x => x.Id),
                StatusCode = StatusCodes.Status200OK
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al agregar ejercicios diarios masivamente.");
            return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<IEnumerable<Guid>>
            {
                Success = false,
                Message = "Ocurrió un error al procesar la solicitud.",
                Data = null,
                StatusCode = StatusCodes.Status400BadRequest
            });
        }
    }
}

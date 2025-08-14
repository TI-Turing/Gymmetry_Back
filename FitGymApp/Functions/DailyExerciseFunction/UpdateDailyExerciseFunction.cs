using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Domain.DTO.DailyExercise.Request;
using Gymmetry.Domain.DTO;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Gymmetry.Utils;
using Microsoft.Azure.Functions.Worker.Http;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Gymmetry.Functions.DailyExerciseFunction;

public class UpdateDailyExerciseFunction
{
    private readonly ILogger<UpdateDailyExerciseFunction> _logger;
    private readonly IDailyExerciseService _service;

    public UpdateDailyExerciseFunction(ILogger<UpdateDailyExerciseFunction> logger, IDailyExerciseService service)
    {
        _logger = logger;
        _service = service;
    }

    [Function("DailyExercise_UpdateDailyExerciseFunction")]
    public async Task<HttpResponseData> UpdateAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "dailyexercise/update")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("DailyExercise_UpdateDailyExerciseFunction");
        if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
        {
            var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
            await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<bool>
            {
                Success = false,
                Message = error!,
                Data = false,
                StatusCode = StatusCodes.Status401Unauthorized
            });
            return unauthorizedResponse;
        }
        logger.LogInformation("Procesando solicitud para actualizar un DailyExercise.");
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var objRequest = JsonConvert.DeserializeObject<UpdateDailyExerciseRequest>(requestBody);
            var validationResult = ModelValidator.ValidateModel<UpdateDailyExerciseRequest, bool>(objRequest, StatusCodes.Status400BadRequest);
            if (validationResult is not null)
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(validationResult);
                return badResponse;
            }
            var ip = FunctionResponseHelper.GetClientIp(req);
            var result = await _service.UpdateDailyExerciseAsync(objRequest, userId, ip);
            if (!result.Success)
            {
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = result.Message,
                    Data = false,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
            var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await successResponse.WriteAsJsonAsync(new ApiResponse<bool>
            {
                Success = true,
                Message = result.Message,
                Data = true,
                StatusCode = StatusCodes.Status200OK
            });
            return successResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al actualizar DailyExercise.");
            var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new ApiResponse<bool>
            {
                Success = false,
                Message = "Ocurrió un error al procesar la solicitud.",
                Data = false,
                StatusCode = StatusCodes.Status400BadRequest
            });
            return errorResponse;
        }
    }
}

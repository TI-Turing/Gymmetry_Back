using Gymmetry.Domain.DTO.UserExerciseMax.Request;
using Gymmetry.Domain.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Utils;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Gymmetry.Functions.UserExerciseMaxFunction;

public class UpdateUserExerciseMaxFunction
{
    private readonly ILogger<UpdateUserExerciseMaxFunction> _logger;
    private readonly IUserExerciseMaxService _service;

    public UpdateUserExerciseMaxFunction(ILogger<UpdateUserExerciseMaxFunction> logger, IUserExerciseMaxService service)
    {
        _logger = logger;
        _service = service;
    }

    [Function("UserExerciseMax_UpdateFunction")]
    public async Task<HttpResponseData> UpdateAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "userexercisemax/update")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("UserExerciseMax_UpdateFunction");
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
        logger.LogInformation("Procesando solicitud para actualizar un UserExerciseMax.");
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var objRequest = JsonConvert.DeserializeObject<UpdateUserExerciseMaxRequest>(requestBody);
            var validationResult = ModelValidator.ValidateModel<UpdateUserExerciseMaxRequest, bool>(objRequest, StatusCodes.Status400BadRequest);
            if (validationResult is not null)
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(validationResult);
                return badResponse;
            }
            var ip = FunctionResponseHelper.GetClientIp(req);
            var result = await _service.UpdateAsync(objRequest, userId, ip);
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
            logger.LogError(ex, "Error al actualizar UserExerciseMax.");
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

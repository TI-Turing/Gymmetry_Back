using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Gymmetry.Domain.DTO.UserExerciseMax.Request;
using Newtonsoft.Json;
using Gymmetry.Domain.DTO;
using Gymmetry.Application.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Gymmetry.Utils;

namespace Gymmetry.Functions.UserExerciseMaxFunction;

public class AddUserExerciseMaxFunction
{
    private readonly ILogger<AddUserExerciseMaxFunction> _logger;
    private readonly IUserExerciseMaxService _service;

    public AddUserExerciseMaxFunction(ILogger<AddUserExerciseMaxFunction> logger, IUserExerciseMaxService service)
    {
        _logger = logger;
        _service = service;
    }

    [Function("UserExerciseMax_AddFunction")]
    public async Task<HttpResponseData> AddAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "userexercisemax/add")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("UserExerciseMax_AddFunction");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var objRequest = JsonConvert.DeserializeObject<AddUserExerciseMaxRequest>(requestBody);
        FunctionResponseHelper.LogRequest(logger, "UserExerciseMax_AddFunction", objRequest);
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
            var validationResult = ModelValidator.ValidateModel<AddUserExerciseMaxRequest, Guid>(objRequest, StatusCodes.Status400BadRequest);
            if (validationResult is not null)
            {
                return await FunctionResponseHelper.CreateResponseAsync(req, System.Net.HttpStatusCode.BadRequest, validationResult);
            }
            objRequest.Ip = FunctionResponseHelper.GetClientIp(req);
            var result = await _service.CreateAsync(objRequest);
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
            FunctionResponseHelper.LogError(logger, "UserExerciseMax_AddFunction", ex);
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
}

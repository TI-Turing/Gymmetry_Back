using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Gymmetry.Domain.DTO.Daily.Request;
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

namespace Gymmetry.Functions.DailyFunction;

public class AddDailyFunction
{
    private readonly ILogger<AddDailyFunction> _logger;
    private readonly IDailyService _service;

    public AddDailyFunction(ILogger<AddDailyFunction> logger, IDailyService service)
    {
        _logger = logger;
        _service = service;
    }

    [Function("Daily_AddDailyFunction")]
    public async Task<HttpResponseData> AddAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "daily/add")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("Daily_AddDailyFunction");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var objRequest = JsonConvert.DeserializeObject<AddDailyRequest>(requestBody);
        FunctionResponseHelper.LogRequest(logger, "Daily_AddDailyFunction", objRequest);
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
            var validationResult = ModelValidator.ValidateModel<AddDailyRequest, Guid>(objRequest, StatusCodes.Status400BadRequest);
            if (validationResult is not null)
            {
                return await FunctionResponseHelper.CreateResponseAsync(req, System.Net.HttpStatusCode.BadRequest, validationResult);
            }
            var ip = FunctionResponseHelper.GetClientIp(req);
            objRequest.Ip = ip;
            var result = await _service.CreateDailyAsync(objRequest);
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
            FunctionResponseHelper.LogError(logger, "Daily_AddDailyFunction", ex);
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

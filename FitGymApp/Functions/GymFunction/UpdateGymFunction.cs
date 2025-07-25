using Gymmetry.Domain.DTO.Gym.Request;
using Gymmetry.Domain.DTO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Gymmetry.Application.Services.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Gymmetry.Utils;
using Microsoft.AspNetCore.Http;

namespace Gymmetry.Functions.GymFunction;

public class UpdateGymFunction
{
    private readonly ILogger<UpdateGymFunction> _logger;
    private readonly IGymService _service;

    public UpdateGymFunction(ILogger<UpdateGymFunction> logger, IGymService service)
    {
        _logger = logger;
        _service = service;
    }

    [Function("Gym_UpdateGymFunction")]
    public async Task<HttpResponseData> UpdateAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "gym/update")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("Gym_UpdateGymFunction");
        logger.LogInformation("Procesando solicitud para actualizar un Gym.");
        var invocationId = executionContext.InvocationId;

        if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
        {
            var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
            await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<Guid>
            {
                Success = false,
                Message = error!,
                Data = default,
                StatusCode = StatusCodes.Status401Unauthorized
            });
            return unauthorizedResponse;
        }
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var objRequest = JsonConvert.DeserializeObject<UpdateGymRequest>(requestBody);
        var validationResult = ModelValidator.ValidateModel<UpdateGymRequest, Guid>(objRequest, StatusCodes.Status400BadRequest);
        if (validationResult is not null)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(validationResult);
            return badResponse;
        }
        string? ip = req.Headers.TryGetValues("X-Forwarded-For", out var values) ? values.FirstOrDefault()?.Split(',')[0]?.Trim()
                : req.Headers.TryGetValues("X-Original-For", out var originalForValues) ? originalForValues.FirstOrDefault()?.Split(':')[0]?.Trim()
                : req.Headers.TryGetValues("REMOTE_ADDR", out var remoteValues) ? remoteValues.FirstOrDefault()
                : null;
        if (objRequest != null)
        {
            objRequest.Ip = ip;
        }
        try
        {
            var result = await _service.UpdateGymAsync(objRequest, userId, ip, invocationId);
            var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            await response.WriteAsJsonAsync(new ApiResponse<Guid>
            {
                Success = result.Success,
                Message = result.Message,
                Data = objRequest.Id,
                StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status404NotFound
            });
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al actualizar Gym.");
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new ApiResponse<Guid>
            {
                Success = false,
                Message = "Ocurri� un error al procesar la solicitud.",
                Data = default,
                StatusCode = StatusCodes.Status400BadRequest
            });
            return errorResponse;
        }
    }
}

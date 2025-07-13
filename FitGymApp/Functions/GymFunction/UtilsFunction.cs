using System;
using System.Threading.Tasks;
using FitGymApp.Application.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.IO;
using System.Drawing;
using QRCoder;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using FitGymApp.Domain.DTO.Gym.Request;
using FitGymApp.Utils;

namespace FitGymApp.Functions.GymFunction;

public class UtilsFunction
{
    private readonly ILogger<UtilsFunction> _logger;
    private readonly IGymService _gymService;

    public UtilsFunction(ILogger<UtilsFunction> logger, IGymService gymService)
    {
        _logger = logger;
        _gymService = gymService;
    }

    [Function("Gym_GenerateQrFunction")]
    public async Task<HttpResponseData> GenerateQrAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "gym/generate-qr")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("Gym_GenerateQrFunction");
        logger.LogInformation($"Procesando solicitud para generar QR de Gym");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var requestDto = System.Text.Json.JsonSerializer.Deserialize<GenerateGymQrRequest>(requestBody);

        var validationResult = ModelValidator.ValidateModel<GenerateGymQrRequest, byte[]>(requestDto, StatusCodes.Status400BadRequest);
        if (validationResult is not null)
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteAsJsonAsync(validationResult);
            return badResponse;
        }

        var qrResult = await _gymService.GenerateGymQrAsync(requestDto.GymId);
        if (!qrResult.Success || qrResult.Data == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync(qrResult.Message ?? "No se pudo generar el QR");
            return notFoundResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "image/png");
        await response.Body.WriteAsync(qrResult.Data, 0, qrResult.Data.Length);
        return response;
    }
}

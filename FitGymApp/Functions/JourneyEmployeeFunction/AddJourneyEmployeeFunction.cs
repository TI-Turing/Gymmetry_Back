using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FitGymApp.Domain.DTO.JourneyEmployee.Request;
using Newtonsoft.Json;
using FitGymApp.Domain.DTO;
using FitGymApp.Application.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using FitGymApp.Utils;

namespace FitGymApp.Functions.JourneyEmployeeFunction;

public class AddJourneyEmployeeFunction
{
    private readonly ILogger<AddJourneyEmployeeFunction> _logger;
    private readonly IJourneyEmployeeService _service;

    public AddJourneyEmployeeFunction(ILogger<AddJourneyEmployeeFunction> logger, IJourneyEmployeeService service)
    {
        _logger = logger;
        _service = service;
    }

    [Function("JourneyEmployee_AddJourneyEmployeeFunction")]
    public async Task<ApiResponse<Guid>> AddAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "journeyemployee/add")] HttpRequest req)
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

        _logger.LogInformation("Procesando solicitud para agregar un JourneyEmployee.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var objRequest = JsonConvert.DeserializeObject<AddJourneyEmployeeRequest>(requestBody);
            var validationResult = ModelValidator.ValidateModel<AddJourneyEmployeeRequest, Guid>(objRequest, StatusCodes.Status400BadRequest);
            if (validationResult is not null) return validationResult;

            var result = await _service.CreateJourneyEmployeeAsync(objRequest);
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
                Data = result.Data?.Id ?? default,
                StatusCode = StatusCodes.Status200OK
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar JourneyEmployee.");
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

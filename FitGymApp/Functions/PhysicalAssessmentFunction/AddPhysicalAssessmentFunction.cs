using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FitGymApp.Domain.DTO.PhysicalAssessment.Request;
using Newtonsoft.Json;
using FitGymApp.Domain.DTO;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using FitGymApp.Utils;

namespace FitGymApp.Functions.PhysicalAssessmentFunction
{
    public class AddPhysicalAssessmentFunction
    {
        private readonly ILogger<AddPhysicalAssessmentFunction> _logger;
        private readonly IPhysicalAssessmentService _service;

        public AddPhysicalAssessmentFunction(ILogger<AddPhysicalAssessmentFunction> logger, IPhysicalAssessmentService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("PhysicalAssessment_AddPhysicalAssessmentFunction")]
        public async Task<ApiResponse<Guid>> AddAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "physicalassessment/add")] HttpRequest req)
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

            _logger.LogInformation("Procesando solicitud para agregar un PhysicalAssessment.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var objRequest = JsonConvert.DeserializeObject<AddPhysicalAssessmentRequest>(requestBody);

                var validationResult = ModelValidator.ValidateModel<AddPhysicalAssessmentRequest, Guid>(objRequest, StatusCodes.Status400BadRequest);
                if (validationResult is not null) return validationResult;

                var result = await _service.CreatePhysicalAssessmentAsync(objRequest);
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
                _logger.LogError(ex, "Error al agregar PhysicalAssessment.");
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
}

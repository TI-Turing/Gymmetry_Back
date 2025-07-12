using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO;
using FitGymApp.Domain.Models;
using System;
using System.Threading.Tasks;
using FitGymApp.Utils;

namespace FitGymApp.Functions.PlanFunction
{
    public class DeletePlanFunction
    {
        private readonly ILogger<DeletePlanFunction> _logger;
        private readonly IPlanService _service;

        public DeletePlanFunction(ILogger<DeletePlanFunction> logger, IPlanService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("DeletePlanFunction")]
        public async Task<ApiResponse<Guid>> RunAsync([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "plan/{id:guid}")] HttpRequest req, Guid id)
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

            _logger.LogInformation($"Procesando solicitud de borrado para Plan {id}");
            try
            {
                var result = _service.DeletePlan(id);
                if (!result.Success)
                {
                    return new ApiResponse<Guid>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = default,
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }
                return new ApiResponse<Guid>
                {
                    Success = true,
                    Message = result.Message,
                    Data = id,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar Plan.");
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

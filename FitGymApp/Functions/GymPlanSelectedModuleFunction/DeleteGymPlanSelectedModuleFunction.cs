using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;
using System;
using System.Threading.Tasks;
using Gymmetry.Utils;

namespace Gymmetry.Functions.GymPlanSelectedModuleFunction
{
    public class DeleteGymPlanSelectedModuleFunction
    {
        private readonly ILogger<DeleteGymPlanSelectedModuleFunction> _logger;
        private readonly IGymPlanSelectedModuleService _service;

        public DeleteGymPlanSelectedModuleFunction(ILogger<DeleteGymPlanSelectedModuleFunction> logger, IGymPlanSelectedModuleService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("GymPlanSelectedModule_DeleteGymPlanSelectedModuleFunction")]
        public async Task<ApiResponse<Guid>> RunAsync([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "gymplanselectedmodule/{id:guid}")] HttpRequest req, Guid id)
        {
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = error!,
                    Data = default,
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            _logger.LogInformation($"Procesando solicitud de borrado para GymPlanSelectedModule {id}");
            try
            {
                var result = await _service.DeleteGymPlanSelectedModuleAsync(id);
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
                _logger.LogError(ex, "Error al eliminar GymPlanSelectedModule.");
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "Ocurri� un error al procesar la solicitud.",
                    Data = default,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }
    }
}

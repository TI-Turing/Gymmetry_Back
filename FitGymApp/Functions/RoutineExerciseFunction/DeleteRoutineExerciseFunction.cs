using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO;
using FitGymApp.Domain.Models;
using System;
using System.Threading.Tasks;
using FitGymApp.Utils;

namespace FitGymApp.Functions.RoutineExerciseFunction
{
    public class DeleteRoutineExerciseFunction
    {
        private readonly ILogger<DeleteRoutineExerciseFunction> _logger;
        private readonly IRoutineExerciseService _service;

        public DeleteRoutineExerciseFunction(ILogger<DeleteRoutineExerciseFunction> logger, IRoutineExerciseService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("DeleteRoutineExerciseFunction")]
        public async Task<ApiResponse<Guid>> RunAsync([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "routineexercise/{id:guid}")] HttpRequest req, Guid id)
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

            _logger.LogInformation($"Procesando solicitud de borrado para RoutineExercise {id}");
            try
            {
                var result = _service.DeleteRoutineExercise(id);
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
                _logger.LogError(ex, "Error al eliminar RoutineExercise.");
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

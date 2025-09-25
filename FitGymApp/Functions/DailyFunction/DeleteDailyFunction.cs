using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;
using System;
using System.Threading.Tasks;
using Gymmetry.Utils;
using Microsoft.Azure.Functions.Worker.Http;

namespace Gymmetry.Functions.DailyFunction
{
    public class DeleteDailyFunction
    {
        private readonly ILogger<DeleteDailyFunction> _logger;
        private readonly IDailyService _service;

        public DeleteDailyFunction(ILogger<DeleteDailyFunction> logger, IDailyService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("Daily_DeleteDailyFunction")]
        public async Task<HttpResponseData> DeleteAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "daily/{id:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid id)
        {
            var logger = executionContext.GetLogger("Daily_DeleteDailyFunction");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<Guid>
                {
                    Success = false,
                    Message = error!,
                    Data = default,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }
            logger.LogInformation($"Procesando solicitud de borrado para Daily {id}");
            try
            {
                var result = await _service.DeleteDailyAsync(id);
                if (!result.Success)
                {
                    var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new ApiResponse<Guid>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = default,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                    return notFoundResponse;
                }
                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<Guid>
                {
                    Success = true,
                    Message = result.Message,
                    Data = id,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar Daily.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = default,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }
    }
}

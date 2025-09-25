using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using System;
using System.Threading.Tasks;
using Gymmetry.Utils;

namespace Gymmetry.Functions.UserBlockFunction
{
    public class DeleteUserBlockFunction
    {
        private readonly ILogger<DeleteUserBlockFunction> _logger;
        private readonly IUserBlockService _service;

        public DeleteUserBlockFunction(ILogger<DeleteUserBlockFunction> logger, IUserBlockService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("UserBlock_DeleteUserBlockFunction")]
        public async Task<HttpResponseData> DeleteAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "userblock/{blockedUserId:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid blockedUserId)
        {
            var logger = executionContext.GetLogger("UserBlock_DeleteUserBlockFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = error!,
                    Data = false,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            if (!userId.HasValue)
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Usuario no identificado",
                    Data = false,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            logger.LogInformation($"Procesando solicitud de desbloqueo de usuario {blockedUserId}");
            
            try
            {
                var result = await _service.UnblockUserAsync(userId.Value, blockedUserId);
                
                if (!result.Success)
                {
                    var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = false,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                    return notFoundResponse;
                }

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = true,
                    Message = result.Message,
                    Data = true,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al desbloquear usuario");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = false,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }
    }
}
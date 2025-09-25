using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.UserBlock;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Gymmetry.Utils;

namespace Gymmetry.Functions.UserBlockFunction
{
    public class GetUserBlockStatsFunction
    {
        private readonly ILogger<GetUserBlockStatsFunction> _logger;
        private readonly IUserBlockService _service;

        public GetUserBlockStatsFunction(ILogger<GetUserBlockStatsFunction> logger, IUserBlockService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("UserBlock_GetStatsFunction")]
        public async Task<HttpResponseData> GetStatsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "userblock/stats")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UserBlock_GetStatsFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<UserBlockStatsResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            if (!userId.HasValue)
            {
                var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<UserBlockStatsResponse>
                {
                    Success = false,
                    Message = "Usuario no identificado",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            logger.LogInformation($"Consultando estadísticas de UserBlock para usuario: {userId.Value}");
            
            try
            {
                var result = await _service.GetStatsAsync(userId.Value);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<UserBlockStatsResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar estadísticas de UserBlock.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<UserBlockStatsResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }
    }
}
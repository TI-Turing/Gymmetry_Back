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
    public class BulkUnblockUserFunction
    {
        private readonly ILogger<BulkUnblockUserFunction> _logger;
        private readonly IUserBlockService _service;

        public BulkUnblockUserFunction(ILogger<BulkUnblockUserFunction> logger, IUserBlockService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("UserBlock_BulkUnblockFunction")]
        public async Task<HttpResponseData> BulkUnblockAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "userblock/bulk/unblock")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UserBlock_BulkUnblockFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<BulkUnblockResponse>
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
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<BulkUnblockResponse>
                {
                    Success = false,
                    Message = "Usuario no identificado",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            logger.LogInformation($"Procesando desbloqueo masivo para usuario: {userId.Value}");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var objRequest = JsonSerializer.Deserialize<BulkUnblockRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var validation = ModelValidator.ValidateModel<BulkUnblockRequest, BulkUnblockResponse>(objRequest, StatusCodes.Status400BadRequest);
                if (validation != null)
                {
                    var validationResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await validationResponse.WriteAsJsonAsync(validation);
                    return validationResponse;
                }

                var result = await _service.BulkUnblockAsync(objRequest!, userId.Value);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<BulkUnblockResponse>
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
                logger.LogError(ex, "Error en desbloqueo masivo");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<BulkUnblockResponse>
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
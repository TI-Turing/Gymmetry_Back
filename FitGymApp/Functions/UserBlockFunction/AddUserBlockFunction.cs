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
    public class AddUserBlockFunction
    {
        private readonly ILogger<AddUserBlockFunction> _logger;
        private readonly IUserBlockService _service;

        public AddUserBlockFunction(ILogger<AddUserBlockFunction> logger, IUserBlockService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("UserBlock_AddUserBlockFunction")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "userblock")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UserBlock_AddUserBlockFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<UserBlockResponse>
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
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<UserBlockResponse>
                {
                    Success = false,
                    Message = "Usuario no identificado",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            logger.LogInformation("Procesando solicitud de bloqueo de usuario");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var objRequest = JsonSerializer.Deserialize<UserBlockCreateRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var validation = ModelValidator.ValidateModel<UserBlockCreateRequest, UserBlockResponse>(objRequest, StatusCodes.Status400BadRequest);
                if (validation != null)
                {
                    var validationResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await validationResponse.WriteAsJsonAsync(validation);
                    return validationResponse;
                }

                var ip = FunctionResponseHelper.GetClientIp(req);
                var result = await _service.BlockUserAsync(objRequest!, userId.Value, ip);
                
                if (!result.Success)
                {
                    var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await errorResponse.WriteAsJsonAsync(new ApiResponse<UserBlockResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return errorResponse;
                }

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<UserBlockResponse>
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
                logger.LogError(ex, "Error al bloquear usuario");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<UserBlockResponse>
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
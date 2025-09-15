using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.PostShare.Request;
using Gymmetry.Utils;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Gymmetry.Functions.PostShareFunction
{
    public class UpdatePostShareFunction
    {
        private readonly IPostShareService _postShareService;
        private readonly ILogger<UpdatePostShareFunction> _logger;

        public UpdatePostShareFunction(IPostShareService postShareService, ILogger<UpdatePostShareFunction> logger)
        {
            _postShareService = postShareService;
            _logger = logger;
        }

        [Function("PostShare_UpdatePostShareFunction")]
        public async Task<HttpResponseData> UpdateAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "postShare/update")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("PostShare_UpdatePostShareFunction");
            logger.LogInformation("Procesando solicitud para actualizar PostShare.");

            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = error!,
                    Data = false,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = JsonSerializer.Deserialize<UpdatePostShareRequest>(requestBody);
                
                if (request == null)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Request body inválido.",
                        Data = false,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badRequestResponse;
                }

                var result = await _postShareService.UpdatePostShareAsync(request);

                var responseCode = result.Success ? HttpStatusCode.OK : 
                                 result.ErrorCode == "NotFound" ? HttpStatusCode.NotFound : 
                                 HttpStatusCode.BadRequest;

                var response = req.CreateResponse(responseCode);
                await response.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status200OK : 
                                result.ErrorCode == "NotFound" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest
                });

                logger.LogInformation("PostShare actualizado exitosamente");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar PostShare");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = false,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
                return errorResponse;
            }
        }
    }
}
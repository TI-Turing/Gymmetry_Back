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
using Gymmetry.Utils;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Gymmetry.Functions.PostShareFunction
{
    public class DeletePostShareFunction
    {
        private readonly IPostShareService _postShareService;
        private readonly ILogger<DeletePostShareFunction> _logger;

        public DeletePostShareFunction(IPostShareService postShareService, ILogger<DeletePostShareFunction> logger)
        {
            _postShareService = postShareService;
            _logger = logger;
        }

        [Function("PostShare_DeletePostShareFunction")]
        public async Task<HttpResponseData> DeleteAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "postShare/delete")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("PostShare_DeletePostShareFunction");
            logger.LogInformation("Procesando solicitud para eliminar PostShare.");

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
                var requestData = JsonSerializer.Deserialize<JsonElement>(requestBody);
                
                if (!requestData.TryGetProperty("Id", out var idProperty) || 
                    !Guid.TryParse(idProperty.GetString(), out var id))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "ID inválido o faltante.",
                        Data = false,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badRequestResponse;
                }

                var result = await _postShareService.DeletePostShareAsync(id);

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

                logger.LogInformation("PostShare eliminado exitosamente");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar PostShare");
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
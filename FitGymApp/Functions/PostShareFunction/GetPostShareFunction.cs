using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Web;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.PostShare.Response;
using Gymmetry.Utils;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Gymmetry.Functions.PostShareFunction
{
    public class GetPostShareFunction
    {
        private readonly IPostShareService _postShareService;
        private readonly ILogger<GetPostShareFunction> _logger;

        public GetPostShareFunction(IPostShareService postShareService, ILogger<GetPostShareFunction> logger)
        {
            _postShareService = postShareService;
            _logger = logger;
        }

        [Function("PostShare_GetPostShareByIdFunction")]
        public async Task<HttpResponseData> GetByIdAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "postShare/getById")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("PostShare_GetPostShareByIdFunction");
            logger.LogInformation("Procesando solicitud para obtener PostShare por ID.");

            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<PostShareResponse?>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            try
            {
                var query = HttpUtility.ParseQueryString(req.Url.Query);
                var idString = query["id"];
                
                if (string.IsNullOrEmpty(idString) || !Guid.TryParse(idString, out var id))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<PostShareResponse?>
                    {
                        Success = false,
                        Message = "ID inválido o faltante en query parameter.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badRequestResponse;
                }

                var result = await _postShareService.GetPostShareByIdAsync(id);

                var responseCode = result.Success ? HttpStatusCode.OK : 
                                 result.ErrorCode == "NotFound" ? HttpStatusCode.NotFound : 
                                 HttpStatusCode.BadRequest;

                var response = req.CreateResponse(responseCode);
                await response.WriteAsJsonAsync(new ApiResponse<PostShareResponse?>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = result.Success ? StatusCodes.Status200OK : 
                                result.ErrorCode == "NotFound" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest
                });

                logger.LogInformation("PostShare obtenido exitosamente");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener PostShare por ID");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<PostShareResponse?>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
                return errorResponse;
            }
        }
    }
}
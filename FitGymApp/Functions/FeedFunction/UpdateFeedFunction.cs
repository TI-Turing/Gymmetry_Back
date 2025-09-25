using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Feed.Request;
using Gymmetry.Utils;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Gymmetry.Functions.FeedFunction
{
    public class UpdateFeedFunction
    {
        private readonly ILogger<UpdateFeedFunction> _logger;
        private readonly IFeedService _feedService;

        public UpdateFeedFunction(ILogger<UpdateFeedFunction> logger, IFeedService feedService)
        {
            _logger = logger;
            _feedService = feedService;
        }

        [Function("Feed_UpdateFeedFunction")]
        public async Task<HttpResponseData> UpdateFeedAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "feed/{id:guid}")] HttpRequestData req,
            Guid id,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Feed_UpdateFeedFunction");
            logger.LogInformation("Procesando solicitud para actualizar feed {FeedId}", id);

            // Validar JWT
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.Unauthorized, new ApiResponse<object>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }

            try
            {
                // Leer y deserializar el body
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                logger.LogDebug("Request body recibido: {BodyLength} caracteres", body?.Length ?? 0);

                if (string.IsNullOrWhiteSpace(body))
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "El cuerpo de la solicitud está vacío.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                // Configurar opciones de deserialización más flexibles
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                FeedUpdateRequestDto? dto;
                try
                {
                    dto = JsonSerializer.Deserialize<FeedUpdateRequestDto>(body, jsonOptions);
                }
                catch (JsonException jsonEx)
                {
                    logger.LogWarning(jsonEx, "Error al deserializar JSON: {Error}", jsonEx.Message);
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Formato JSON inválido: {jsonEx.Message}",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                if (dto == null)
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Los datos de actualización son nulos o inválidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                // Obtener IP del cliente
                var clientIp = FunctionResponseHelper.GetClientIp(req);

                // Llamar al servicio con toda la lógica de negocio
                var result = await _feedService.UpdateFeedFromRequestAsync(id, dto, userId!.Value, clientIp, executionContext.InvocationId);

                // Retornar respuesta basada en el resultado del servicio
                var statusCode = result.Success ? HttpStatusCode.OK : 
                    (result.ErrorCode == "NotFound" ? HttpStatusCode.NotFound :
                     result.ErrorCode == "Forbidden" ? HttpStatusCode.Forbidden : HttpStatusCode.BadRequest);
                
                var httpStatusCode = result.Success ? StatusCodes.Status200OK : 
                    (result.ErrorCode == "NotFound" ? StatusCodes.Status404NotFound :
                     result.ErrorCode == "Forbidden" ? StatusCodes.Status403Forbidden : StatusCodes.Status400BadRequest);

                return await FunctionResponseHelper.CreateResponseAsync(req, statusCode, new ApiResponse<object>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = httpStatusCode
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado al actualizar feed {FeedId}", id);
                
                return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Ocurrió un error interno al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}

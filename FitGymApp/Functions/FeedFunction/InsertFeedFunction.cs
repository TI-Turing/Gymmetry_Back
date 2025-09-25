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
using System.Linq;

namespace Gymmetry.Functions.FeedFunction
{
    public class InsertFeedFunction
    {
        private readonly ILogger<InsertFeedFunction> _logger;
        private readonly IFeedService _feedService;

        public InsertFeedFunction(ILogger<InsertFeedFunction> logger, IFeedService feedService)
        {
            _logger = logger;
            _feedService = feedService;
        }

        [Function("Feed_CreateFeedFunction")]
        public async Task<HttpResponseData> CreateFeedAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "feed")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Feed_CreateFeedFunction");
            logger.LogInformation("Procesando solicitud para crear un nuevo feed");

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
                // Leer el body de la request
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

                // Deserializar JSON con opciones flexibles
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                FeedCreateRequestDto? dto;
                try
                {
                    dto = JsonSerializer.Deserialize<FeedCreateRequestDto>(body, jsonOptions);
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
                        Message = "Los datos del feed son nulos o inválidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                // Obtener IP del cliente
                var clientIp = FunctionResponseHelper.GetClientIp(req);

                // Llamar al servicio con toda la lógica de negocio
                var result = await _feedService.CreateFeedFromRequestAsync(dto, userId!.Value, clientIp);

                // Retornar respuesta basada en el resultado del servicio
                var statusCode = result.Success ? HttpStatusCode.Created : HttpStatusCode.BadRequest;
                var httpStatusCode = result.Success ? StatusCodes.Status201Created : StatusCodes.Status400BadRequest;

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
                logger.LogError(ex, "Error inesperado al crear feed");
                
                return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Ocurrió un error interno al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        [Function("Feed_UploadMediaFunction")]
        public async Task<HttpResponseData> UploadFeedMediaAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "feed/upload-media")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Feed_UploadMediaFunction");
            logger.LogInformation("Processing request to upload feed media");

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
                string? contentType = req.Headers.TryGetValues("Content-Type", out var ctValues) ? ctValues.FirstOrDefault() : null;

                if (string.IsNullOrEmpty(contentType) || !contentType.Contains("multipart/form-data"))
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Content-Type debe ser multipart/form-data para subir archivos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                var boundary = MultipartRequestHelper.GetBoundary(contentType, 4096);
                var sections = await MultipartRequestHelper.GetSectionsAsync(req.Body, boundary);

                var feedId = Guid.Empty;
                byte[]? mediaBytes = null;
                string? fileName = null;
                string? mediaType = null;

                foreach (var section in sections)
                {
                    if (section.Name == "FeedId")
                    {
                        if (!Guid.TryParse(section.Value, out feedId))
                        {
                            return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<object>
                            {
                                Success = false,
                                Message = "FeedId debe ser un GUID válido.",
                                Data = null,
                                StatusCode = StatusCodes.Status400BadRequest
                            });
                        }
                    }
                    if (section.Name == "Media" && section.FileContent != null)
                    {
                        mediaBytes = section.FileContent;
                        fileName = section.FileName;
                        mediaType = section.ContentType;
                    }
                }

                if (feedId == Guid.Empty)
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "FeedId es requerido.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                if (mediaBytes == null || mediaBytes.Length == 0)
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Media es requerido y no puede estar vacío.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                // Validar tamaño de archivo
                const int maxFileSize = 50 * 1024 * 1024; // 50MB
                if (mediaBytes.Length > maxFileSize)
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"El archivo es demasiado grande. Máximo permitido: {maxFileSize / (1024 * 1024)}MB.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                logger.LogInformation("Subiendo media para Feed {FeedId}: {FileSize} bytes, tipo: {MediaType}",
                    feedId, mediaBytes.Length, mediaType ?? "unknown");

                // Llamar al servicio (la lógica de negocio ya está en el servicio)
                var result = await _feedService.UploadFeedMediaAsync(feedId, mediaBytes, fileName, mediaType);

                var statusCode = result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
                var httpStatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest;

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
                logger.LogError(ex, "Error inesperado al subir media de feed");

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

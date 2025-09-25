using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Feed.Request;
using Gymmetry.Domain.DTO.Feed.Response;
using Gymmetry.Utils;
using Microsoft.AspNetCore.Http;

namespace Gymmetry.Functions.FeedFunction
{
    public class CreateFeedWithMediaFunction
    {
        private readonly ILogger<CreateFeedWithMediaFunction> _logger;
        private readonly IFeedService _feedService;

        public CreateFeedWithMediaFunction(ILogger<CreateFeedWithMediaFunction> logger, IFeedService feedService)
        {
            _logger = logger;
            _feedService = feedService;
        }

        [Function("Feed_CreateWithMediaFunction")]
        public async Task<HttpResponseData> CreateFeedWithMediaAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "feed/create-with-media")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Feed_CreateWithMediaFunction");
            logger.LogInformation("Procesando solicitud para crear feed con multimedia");

            // Validar JWT
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.Unauthorized, new ApiResponse<FeedWithMediaResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }

            try
            {
                // Verificar Content-Type
                string? contentType = req.Headers.TryGetValues("Content-Type", out var ctValues) ? ctValues.FirstOrDefault() : null;

                if (string.IsNullOrEmpty(contentType) || !contentType.Contains("multipart/form-data"))
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<FeedWithMediaResponse>
                    {
                        Success = false,
                        Message = "Content-Type debe ser multipart/form-data para subir archivos multimedia.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                // Parsear multipart request
                var boundary = MultipartRequestHelper.GetBoundary(contentType, 4096);
                var sections = await MultipartRequestHelper.GetSectionsAsync(req.Body, boundary);

                // Extraer datos del formulario
                var request = ExtractRequestFromMultipart(sections);
                if (request == null)
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<FeedWithMediaResponse>
                    {
                        Success = false,
                        Message = "Datos de formulario inválidos o incompletos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                // Validaciones básicas
                if (request.Files == null || !request.Files.Any())
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<FeedWithMediaResponse>
                    {
                        Success = false,
                        Message = "Al menos un archivo multimedia es requerido.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                if (request.Files.Length > 5)
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<FeedWithMediaResponse>
                    {
                        Success = false,
                        Message = "Máximo 5 archivos permitidos por post.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }

                // Validar tamaño total
                var totalSize = request.Files.Sum(f => f.Length);
                const long maxTotalSize = 25 * 1024 * 1024; // 25MB
                if (totalSize > maxTotalSize)
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.RequestEntityTooLarge, new ApiResponse<FeedWithMediaResponse>
                    {
                        Success = false,
                        Message = $"El tamaño total de archivos ({totalSize / (1024 * 1024)}MB) excede el límite de {maxTotalSize / (1024 * 1024)}MB.",
                        Data = null,
                        StatusCode = StatusCodes.Status413PayloadTooLarge
                    });
                }

                logger.LogInformation("Creando feed con multimedia para usuario {UserId}: {FileCount} archivos, {TotalSize} bytes", 
                    userId, request.Files.Length, totalSize);

                // Obtener IP del cliente
                var clientIp = FunctionResponseHelper.GetClientIp(req);

                // Llamar al servicio
                var result = await _feedService.CreateFeedWithMediaAsync(request, userId!.Value, clientIp);

                // Determinar códigos de respuesta apropiados
                var statusCode = result.Success ? HttpStatusCode.Created : 
                    (result.ErrorCode == "BadRequest" ? HttpStatusCode.BadRequest : 
                     result.ErrorCode == "TechnicalError" ? HttpStatusCode.InternalServerError : HttpStatusCode.BadRequest);

                var httpStatusCode = result.Success ? StatusCodes.Status201Created : 
                    (result.ErrorCode == "BadRequest" ? StatusCodes.Status400BadRequest : 
                     result.ErrorCode == "TechnicalError" ? StatusCodes.Status500InternalServerError : StatusCodes.Status400BadRequest);

                return await FunctionResponseHelper.CreateResponseAsync(req, statusCode, new ApiResponse<FeedWithMediaResponse>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = httpStatusCode
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error inesperado al crear feed con multimedia");

                return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.InternalServerError, new ApiResponse<FeedWithMediaResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error interno al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
            }
        }

        private CreateFeedWithMediaRequest? ExtractRequestFromMultipart(System.Collections.Generic.List<MultipartSectionData> sections)
        {
            try
            {
                var description = string.Empty;
                var isAnonymous = false;
                var hashtags = string.Empty;
                var files = new System.Collections.Generic.List<FileData>();

                foreach (var section in sections)
                {
                    switch (section.Name?.ToLower())
                    {
                        case "description":
                            description = section.Value ?? string.Empty;
                            break;
                        case "isanonymous":
                            bool.TryParse(section.Value, out isAnonymous);
                            break;
                        case "hashtags":
                            hashtags = section.Value ?? string.Empty;
                            break;
                        case "files":
                            if (section.FileContent != null && !string.IsNullOrEmpty(section.FileName))
                            {
                                files.Add(new FileData
                                {
                                    Content = section.FileContent,
                                    FileName = section.FileName,
                                    ContentType = section.ContentType ?? "application/octet-stream"
                                });
                            }
                            break;
                    }
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    return null; // Descripción es requerida
                }

                return new CreateFeedWithMediaRequest
                {
                    Description = description,
                    IsAnonymous = isAnonymous,
                    Hashtags = string.IsNullOrWhiteSpace(hashtags) ? null : hashtags,
                    Files = files.ToArray()
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
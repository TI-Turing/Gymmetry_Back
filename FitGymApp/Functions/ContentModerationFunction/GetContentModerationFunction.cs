using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ContentModeration;
using Gymmetry.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Utils;

namespace Gymmetry.Functions.ContentModerationFunction
{
    public class GetContentModerationFunction
    {
        private readonly ILogger<GetContentModerationFunction> _logger;
        private readonly IContentModerationService _service;

        public GetContentModerationFunction(ILogger<GetContentModerationFunction> logger, IContentModerationService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("ContentModeration_GetContentModerationByIdFunction")]
        public async Task<HttpResponseData> GetByIdAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "contentmoderation/{id:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid id)
        {
            var logger = executionContext.GetLogger("ContentModeration_GetContentModerationByIdFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation($"Consultando ContentModeration por Id: {id}");
            
            try
            {
                var result = await _service.GetByIdAsync(id);
                
                if (!result.Success)
                {
                    var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                    return notFoundResponse;
                }

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
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
                logger.LogError(ex, "Error al consultar ContentModeration por Id.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("ContentModeration_GetPagedFunction")]
        public async Task<HttpResponseData> GetPagedAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "contentmoderation")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ContentModeration_GetPagedFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationListResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation("Consultando ContentModeration paginado");
            
            try
            {
                var query = System.Web.HttpUtility.ParseQueryString(new Uri(req.Url.ToString()).Query);
                int page = int.TryParse(query.Get("page"), out var p) ? p : 1;
                int pageSize = int.TryParse(query.Get("pageSize"), out var ps) ? ps : 50;

                var result = await _service.GetPagedAsync(page, pageSize);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationListResponse>
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
                logger.LogError(ex, "Error al consultar ContentModeration paginado.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationListResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("ContentModeration_GetPendingFunction")]
        public async Task<HttpResponseData> GetPendingAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "contentmoderation/pending")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ContentModeration_GetPendingFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<ContentModerationResponse>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation("Consultando ContentModeration pendientes");
            
            try
            {
                var result = await _service.GetPendingReviewAsync();

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<ContentModerationResponse>>
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
                logger.LogError(ex, "Error al consultar ContentModeration pendientes.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<ContentModerationResponse>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("ContentModeration_GetByContentFunction")]
        public async Task<HttpResponseData> GetByContentAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "contentmoderation/content/{contentId:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid contentId)
        {
            var logger = executionContext.GetLogger("ContentModeration_GetByContentFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation($"Consultando estado de moderación para contenido: {contentId}");
            
            try
            {
                var query = System.Web.HttpUtility.ParseQueryString(new Uri(req.Url.ToString()).Query);
                var contentTypeRaw = query.Get("contentType");
                if (string.IsNullOrWhiteSpace(contentTypeRaw) || !Enum.TryParse<ContentType>(contentTypeRaw, out var contentType))
                {
                    var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
                    {
                        Success = false,
                        Message = "Parámetro contentType requerido y válido (Feed=1, Comment=2)",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badRequestResponse;
                }

                var result = await _service.GetByContentAsync(contentId, contentType);

                if (!result.Success)
                {
                    var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                    return notFoundResponse;
                }

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
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
                logger.LogError(ex, "Error al consultar estado de moderación.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationResponse>
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
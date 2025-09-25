using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ContentModeration;
using Gymmetry.Repository.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Gymmetry.Utils;
using FitGymApp.Utils;

namespace Gymmetry.Functions.ContentModerationFunction
{
    public class ContentModerationUtilityFunction
    {
        private readonly ILogger<ContentModerationUtilityFunction> _logger;
        private readonly IContentModerationService _service;
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;

        public ContentModerationUtilityFunction(ILogger<ContentModerationUtilityFunction> logger, IContentModerationService service, IUserRepository userRepository, IUserTypeRepository userTypeRepository)
        {
            _logger = logger;
            _service = service;
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
        }

        [Function("ContentModeration_DeleteContentModerationFunction")]
        public async Task<HttpResponseData> DeleteAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "contentmoderation/{id:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid id)
        {
            var logger = executionContext.GetLogger("ContentModeration_DeleteContentModerationFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = error!,
                    Data = false,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            var isModerator = await ModeratorValidator.IsModeratorAsync(userId, _userRepository, _userTypeRepository);
            if (!isModerator)
            {
                var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Acceso restringido",
                    Data = false,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation($"Procesando solicitud de borrado para ContentModeration {id}");
            
            try
            {
                var result = await _service.DeleteAsync(id);
                
                if (!result.Success)
                {
                    var notFoundResponse = req.CreateResponse(System.Net.HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = false,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                    return notFoundResponse;
                }

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = true,
                    Message = result.Message,
                    Data = true,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar ContentModeration.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = false,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("ContentModeration_FindContentModerationFunction")]
        public async Task<HttpResponseData> FindAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contentmoderation/find")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ContentModeration_FindContentModerationFunction");
            
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

            var isModerator = await ModeratorValidator.IsModeratorAsync(userId, _userRepository, _userTypeRepository);
            if (!isModerator)
            {
                var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<ContentModerationResponse>>
                {
                    Success = false,
                    Message = "Acceso restringido",
                    Data = null,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation("Procesando búsqueda de ContentModeration");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (filters == null || filters.Count == 0)
                {
                    var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<ContentModerationResponse>>
                    {
                        Success = false,
                        Message = "Filtros vacíos",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badRequestResponse;
                }

                var result = await _service.FindByFieldsAsync(filters);

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
                logger.LogError(ex, "Error al buscar ContentModeration.");
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

        [Function("ContentModeration_GetStatsFunction")]
        public async Task<HttpResponseData> GetStatsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "contentmoderation/stats")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ContentModeration_GetStatsFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationStatsResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            var isModerator = await ModeratorValidator.IsModeratorAsync(userId, _userRepository, _userTypeRepository);
            if (!isModerator)
            {
                var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationStatsResponse>
                {
                    Success = false,
                    Message = "Acceso restringido",
                    Data = null,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation("Consultando estadísticas de ContentModeration");
            
            try
            {
                var result = await _service.GetStatsAsync();

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationStatsResponse>
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
                logger.LogError(ex, "Error al consultar estadísticas de ContentModeration.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<ContentModerationStatsResponse>
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
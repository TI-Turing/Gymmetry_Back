using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.UserBlock;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Utils;

namespace Gymmetry.Functions.UserBlockFunction
{
    public class GetUserBlockFunction
    {
        private readonly ILogger<GetUserBlockFunction> _logger;
        private readonly IUserBlockService _service;

        public GetUserBlockFunction(ILogger<GetUserBlockFunction> logger, IUserBlockService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("UserBlock_GetBlockedUsersFunction")]
        public async Task<HttpResponseData> GetBlockedAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "userblock/blocked")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UserBlock_GetBlockedUsersFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
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
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
                {
                    Success = false,
                    Message = "Usuario no identificado",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            logger.LogInformation($"Consultando usuarios bloqueados para usuario: {userId.Value}");
            
            try
            {
                var query = System.Web.HttpUtility.ParseQueryString(new Uri(req.Url.ToString()).Query);
                int page = int.TryParse(query.Get("page"), out var p) ? p : 1;
                int pageSize = int.TryParse(query.Get("pageSize"), out var ps) ? ps : 50;

                var result = await _service.GetBlockedUsersAsync(userId.Value, page, pageSize);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
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
                logger.LogError(ex, "Error al consultar usuarios bloqueados");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("UserBlock_GetBlockersFunction")]
        public async Task<HttpResponseData> GetBlockersAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "userblock/blockers")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UserBlock_GetBlockersFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
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
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
                {
                    Success = false,
                    Message = "Usuario no identificado",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            logger.LogInformation($"Consultando usuarios que me bloquearon para usuario: {userId.Value}");
            
            try
            {
                var query = System.Web.HttpUtility.ParseQueryString(new Uri(req.Url.ToString()).Query);
                int page = int.TryParse(query.Get("page"), out var p) ? p : 1;
                int pageSize = int.TryParse(query.Get("pageSize"), out var ps) ? ps : 50;

                var result = await _service.GetBlockersAsync(userId.Value, page, pageSize);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
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
                logger.LogError(ex, "Error al consultar bloqueadores");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("UserBlock_CheckBlockStatusFunction")]
        public async Task<HttpResponseData> CheckAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "userblock/check/{targetUserId:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid targetUserId)
        {
            var logger = executionContext.GetLogger("UserBlock_CheckBlockStatusFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<UserBlockCheckResponse>
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
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<UserBlockCheckResponse>
                {
                    Success = false,
                    Message = "Usuario no identificado",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            logger.LogInformation($"Verificando estado de bloqueo entre {userId.Value} y {targetUserId}");
            
            try
            {
                var result = await _service.CheckBlockStatusAsync(userId.Value, targetUserId);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<UserBlockCheckResponse>
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
                logger.LogError(ex, "Error al verificar estado de bloqueo");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<UserBlockCheckResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("UserBlock_GetMutualBlocksFunction")]
        public async Task<HttpResponseData> GetMutualAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "userblock/mutual")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UserBlock_GetMutualBlocksFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
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
                await badRequestResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
                {
                    Success = false,
                    Message = "Usuario no identificado",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return badRequestResponse;
            }

            logger.LogInformation($"Consultando bloqueos mutuos para usuario: {userId.Value}");
            
            try
            {
                var query = System.Web.HttpUtility.ParseQueryString(new Uri(req.Url.ToString()).Query);
                int page = int.TryParse(query.Get("page"), out var p) ? p : 1;
                int pageSize = int.TryParse(query.Get("pageSize"), out var ps) ? ps : 50;

                var result = await _service.GetMutualBlocksAsync(userId.Value, page, pageSize);

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
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
                logger.LogError(ex, "Error al consultar bloqueos mutuos");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<UserBlockResponse>>
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
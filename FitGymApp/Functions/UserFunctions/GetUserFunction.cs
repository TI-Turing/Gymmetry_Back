using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO;
using FitGymApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using FitGymApp.Utils;
using Newtonsoft.Json;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace FitGymApp.Functions.UserFunctions
{
    public class GetUserFunction
    {
        private readonly ILogger<GetUserFunction> _logger;
        private readonly IUserService _userService;

        public GetUserFunction(ILogger<GetUserFunction> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [Function("User_GetUserByIdFunction")]
        public async Task<HttpResponseData> GetUserByIdAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user/{id:guid}")] HttpRequestData req,
            FunctionContext executionContext,
            Guid id)
        {
            var logger = executionContext.GetLogger("User_GetUserByIdFunction");
            logger.LogInformation($"Consultando usuario por Id: {id}");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<User>
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
                var result = await _userService.GetUserByIdAsync(id);
                if (!result.Success)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteAsJsonAsync(new ApiResponse<User>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status404NotFound
                    });
                    return notFoundResponse;
                }
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<User>
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
                logger.LogError(ex, "Error al consultar usuario por Id.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<User>
                {
                    Success = false,
                    Message = "Ocurri� un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("User_GetAllUsersFunction")]
        public async Task<HttpResponseData> GetAllUsersAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("User_GetAllUsersFunction");
            logger.LogInformation("Consultando todos los usuarios activos.");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<User>>
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
                var result = await _userService.GetAllUsersAsync();
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<User>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar todos los usuarios.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<User>>
                {
                    Success = false,
                    Message = "Ocurri� un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }

        [Function("User_FindUsersByFieldsFunction")]
        public async Task<HttpResponseData> FindUsersByFieldsAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users/find")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("User_FindUsersByFieldsFunction");
            logger.LogInformation("Consultando usuarios por filtros din�micos.");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<User>>
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
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                if (filters == null || filters.Count == 0)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<User>>
                    {
                        Success = false,
                        Message = "No se proporcionaron filtros v�lidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return badResponse;
                }
                var result = await _userService.FindUsersByFieldsAsync(filters);
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<User>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al consultar usuarios por filtros.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<User>>
                {
                    Success = false,
                    Message = "Ocurri� un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return errorResponse;
            }
        }
    }
}

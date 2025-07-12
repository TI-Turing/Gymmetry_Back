using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO.Auth.Request;
using FitGymApp.Domain.DTO.Auth.Response;
using FitGymApp.Domain.DTO;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FitGymApp.Functions.Auth
{
    public class RefreshTokenFunction
    {
        private readonly ILogger<RefreshTokenFunction> _logger;
        private readonly IAuthService _authService;

        public RefreshTokenFunction(ILogger<RefreshTokenFunction> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [Function("Auth_RefreshTokenFunction")]
        public async Task<ApiResponse<RefreshTokenResponse>> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user/refresh-token")] HttpRequest req)
        {
            _logger.LogInformation("Procesando solicitud de refresh token");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var refreshRequest = JsonConvert.DeserializeObject<RefreshTokenRequest>(requestBody);
                if (refreshRequest == null)
                {
                    return new ApiResponse<RefreshTokenResponse>
                    {
                        Success = false,
                        Message = "El cuerpo de la solicitud no coincide con la estructura esperada.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _authService.RefreshToken(refreshRequest);
                if (result == null)
                {
                    return new ApiResponse<RefreshTokenResponse>
                    {
                        Success = false,
                        Message = "Refresh token inválido o expirado.",
                        Data = null,
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }
                return new ApiResponse<RefreshTokenResponse>
                {
                    Success = true,
                    Message = "Token refrescado correctamente.",
                    Data = result,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al refrescar el token.");
                return new ApiResponse<RefreshTokenResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
        }
    }
}

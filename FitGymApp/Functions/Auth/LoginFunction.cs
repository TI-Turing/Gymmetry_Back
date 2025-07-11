using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FitGymApp.Domain.DTO.Auth.Request;
using FitGymApp.Domain.DTO.Auth.Response;
using FitGymApp.Domain.DTO;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FitGymApp.Application.Services.Interfaces;

namespace FitGymApp.Functions.Auth
{
    public class LoginFunction
    {
        private readonly ILogger<LoginFunction> _logger;
        private readonly IAuthService _authService;

        public LoginFunction(ILogger<LoginFunction> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [Function("LoginFunction")]
        public async Task<ApiResponse<LoginResponse>> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "login")] HttpRequest req)
        {
            _logger.LogInformation("Procesando login de usuario.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var loginRequest = JsonSerializer.Deserialize<LoginRequest>(requestBody);
                if (loginRequest == null)
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Datos de login inválidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }
                var result = _authService.Login(loginRequest);
                if (result == null)
                {
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Credenciales incorrectas o usuario inactivo.",
                        Data = null,
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }
                return new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Login exitoso.",
                    Data = result,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login.");
                return new ApiResponse<LoginResponse>
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

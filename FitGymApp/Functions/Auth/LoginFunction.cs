using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO;
using FitGymApp.Domain.DTO.Auth.Request;
using FitGymApp.Domain.DTO.Auth.Response;
using FitGymApp.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace FitGymApp.Functions.Auth
{
    public class Auth_LoginFunction
    {
        private readonly ILogger<Auth_LoginFunction> _logger;
        private readonly IAuthService _authService;

        public Auth_LoginFunction(ILogger<Auth_LoginFunction> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [Function("Auth_LoginFunction")]
        public async Task<ApiResponse<LoginResponse>> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "auth/login")] HttpRequest req)
        {
            _logger.LogInformation("Procesando login de usuario.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var loginRequest = JsonConvert.DeserializeObject<LoginRequest>(requestBody);
                var validationResult = ModelValidator.ValidateModel<LoginRequest, LoginResponse>(loginRequest, StatusCodes.Status400BadRequest);
                if (validationResult is not null) return validationResult;

                var result = await _authService.LoginAsync(loginRequest);
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
            catch (System.Exception ex)
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

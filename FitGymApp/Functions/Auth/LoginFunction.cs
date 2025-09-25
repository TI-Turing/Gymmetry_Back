using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Auth.Request;
using Gymmetry.Domain.DTO.Auth.Response;
using Gymmetry.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace Gymmetry.Functions.Auth
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

        private static void ForceCorsHeaders(HttpResponseData resp, string? origin, string? requestedHeaders, ILogger logger)
        {
            // Evitar falta del header en runtime aislado: agregamos directamente aquí además del helper
            var allowOrigin = origin ?? "http://localhost:8081"; // fallback dev
            if (!resp.Headers.Contains("Access-Control-Allow-Origin")) resp.Headers.Add("Access-Control-Allow-Origin", allowOrigin);
            if (!resp.Headers.Contains("Access-Control-Allow-Credentials")) resp.Headers.Add("Access-Control-Allow-Credentials", "true");
            if (!resp.Headers.Contains("Access-Control-Allow-Methods")) resp.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            var baseHeaders = "Content-Type, Authorization, Accept, Cache-Control, Pragma, Expires, X-Requested-With";
            var merged = baseHeaders;
            if (!string.IsNullOrWhiteSpace(requestedHeaders))
            {
                var extras = requestedHeaders.Split(',').Select(h => h.Trim()).Where(h => !string.IsNullOrEmpty(h));
                foreach (var h in extras)
                {
                    if (!merged.Contains(h, System.StringComparison.OrdinalIgnoreCase))
                        merged += ", " + h;
                }
            }
            if (resp.Headers.Contains("Access-Control-Allow-Headers")) resp.Headers.Remove("Access-Control-Allow-Headers");
            resp.Headers.Add("Access-Control-Allow-Headers", merged);
            if (!resp.Headers.Contains("Access-Control-Max-Age")) resp.Headers.Add("Access-Control-Max-Age", "86400");
            if (!resp.Headers.Contains("Vary")) resp.Headers.Add("Vary", "Origin");

            // Log depuración de headers finales
            foreach (var h in resp.Headers)
                logger.LogDebug("[Login CORS] {Key}={Value}", h.Key, string.Join(';', h.Value));
        }

        [Function("Auth_LoginFunction")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/login")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Auth_LoginFunction");
            logger.LogInformation("Procesando login de usuario / manejando preflight.");

            string? origin = null;
            if (req.Headers.TryGetValues("Origin", out var originValues))
                origin = originValues.FirstOrDefault();

            string? requestedHeaders = null;
            if (req.Headers.TryGetValues("Access-Control-Request-Headers", out var acrhValues))
                requestedHeaders = acrhValues.FirstOrDefault();

            if (req.Method.Equals("OPTIONS", System.StringComparison.OrdinalIgnoreCase))
            {
                var preflight = req.CreateResponse(HttpStatusCode.NoContent);
                ForceCorsHeaders(preflight, origin, requestedHeaders, logger);
                return preflight;
            }

            try
            {

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var loginRequest = JsonConvert.DeserializeObject<LoginRequest>(requestBody);
                var validationResult = ModelValidator.ValidateModel<LoginRequest, LoginResponse>(loginRequest, StatusCodes.Status400BadRequest);
                if (validationResult is not null)
                {
                    var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    // Asegurar CORS antes de escribir body
                    ForceCorsHeaders(badResponse, origin, requestedHeaders, logger);
                    await badResponse.WriteAsJsonAsync(validationResult);
                    return badResponse;
                }

                string? ip = req.Headers.TryGetValues("X-Forwarded-For", out var values) ? values.FirstOrDefault()?.Split(',')[0]?.Trim()
                    : req.Headers.TryGetValues("X-Original-For", out var originalForValues) ? originalForValues.FirstOrDefault()?.Split(':')[0]?.Trim()
                    : req.Headers.TryGetValues("REMOTE_ADDR", out var remoteValues) ? remoteValues.FirstOrDefault()
                    : null;

                var result = await _authService.LoginAsync(loginRequest!, ip);
                if (result == null)
                {
                    var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                    ForceCorsHeaders(unauthorizedResponse, origin, requestedHeaders, logger);
                    await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = "Credenciales incorrectas o usuario inactivo.",
                        Data = null,
                        StatusCode = StatusCodes.Status401Unauthorized
                    });
                    return unauthorizedResponse;
                }
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                ForceCorsHeaders(successResponse, origin, requestedHeaders, logger); // headers antes del body
                var options = System.Text.Json.JsonSerializerOptions.Default;
                options = new System.Text.Json.JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                    WriteIndented = true
                };
                var json = System.Text.Json.JsonSerializer.Serialize(new ApiResponse<LoginResponse>
                {
                    Success = true,
                    Message = "Login exitoso.",
                    Data = result,
                    StatusCode = StatusCodes.Status200OK
                }, options);
                await successResponse.WriteStringAsync(json);
                if (!successResponse.Headers.Contains("Content-Type"))
                    successResponse.Headers.Add("Content-Type", "application/json; charset=utf-8");
                return successResponse;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error en login.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                ForceCorsHeaders(errorResponse, origin, requestedHeaders, logger);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<LoginResponse>
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

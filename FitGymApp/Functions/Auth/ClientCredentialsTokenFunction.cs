using System.IO;
using System.Net;
using System.Threading.Tasks;
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

namespace Gymmetry.Functions.Auth
{
    public class Auth_ClientCredentialsTokenFunction
    {
        private readonly ILogger<Auth_ClientCredentialsTokenFunction> _logger;
        private readonly IClientCredentialsService _service;

        public Auth_ClientCredentialsTokenFunction(ILogger<Auth_ClientCredentialsTokenFunction> logger, IClientCredentialsService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("Auth_ClientCredentialsTokenFunction")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/client-credentials/token")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("Procesando token client credentials");
            try
            {
                string body = await new StreamReader(req.Body).ReadToEndAsync();
                var tokenRequest = JsonConvert.DeserializeObject<ClientCredentialsRequest>(body);
                var validation = ModelValidator.ValidateModel<ClientCredentialsRequest, ClientCredentialsResponse>(tokenRequest, StatusCodes.Status400BadRequest);
                if (validation is not null)
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteAsJsonAsync(validation);
                    return bad;
                }
                var result = await _service.GetTokenAsync(tokenRequest);
                var status = result.Success ? HttpStatusCode.OK : (result.ErrorCode == "Unauthorized" ? HttpStatusCode.Unauthorized : HttpStatusCode.BadRequest);
                var response = req.CreateResponse(status);
                await response.WriteAsJsonAsync(new ApiResponse<ClientCredentialsResponse>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = (int)status
                });
                return response;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error emitiendo token client credentials");
                var error = req.CreateResponse(HttpStatusCode.BadRequest);
                await error.WriteAsJsonAsync(new ApiResponse<ClientCredentialsResponse>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
                return error;
            }
        }
    }
}

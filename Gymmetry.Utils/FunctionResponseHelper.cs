using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Utils
{
    public static class FunctionResponseHelper
    {
        public static async Task<HttpResponseData> CreateResponseAsync<T>(HttpRequestData req, HttpStatusCode statusCode, ApiResponse<T> response)
        {
            var res = req.CreateResponse(statusCode);
            await res.WriteAsJsonAsync(response);
            return res;
        }

        public static void LogRequest(ILogger logger, string functionName, object? request)
        {
            logger.LogInformation($"[{functionName}] Request: {System.Text.Json.JsonSerializer.Serialize(request)}");
        }

        public static void LogError(ILogger logger, string functionName, Exception ex)
        {
            logger.LogError(ex, $"[{functionName}] Error: {ex.Message}");
        }
    }
}

using System.Net;
using System.Text.Json;
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
            try
            {
                var payload = request == null ? "<null>" : JsonSerializer.Serialize(request);
                logger.LogInformation("[{Function}] Request: {Payload}", functionName, payload);
            }
            catch
            {
                logger.LogInformation("[{Function}] Request received (serialization failed)", functionName);
            }
        }

        public static void LogError(ILogger logger, string functionName, System.Exception ex)
        {
            logger.LogError(ex, "[{Function}] Error: {Message}", functionName, ex.Message);
        }

        public static string? GetClientIp(HttpRequestData req)
        {
            if (req.Headers.TryGetValues("X-Forwarded-For", out var values))
                return values.FirstOrDefault()?.Split(',')[0]?.Trim();
            if (req.Headers.TryGetValues("X-Original-For", out var originalFor))
                return originalFor.FirstOrDefault()?.Split(',')[0]?.Trim();
            if (req.Headers.TryGetValues("X-Client-IP", out var clientIp))
                return clientIp.FirstOrDefault();
            return null;
        }
    }
}

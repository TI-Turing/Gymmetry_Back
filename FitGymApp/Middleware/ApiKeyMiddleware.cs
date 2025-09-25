using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Gymmetry.Utils;
using Gymmetry.Domain.DTO;
using Microsoft.AspNetCore.Http;

namespace FitGymApp.Middleware
{
    /// <summary>
    /// Middleware que exige un parámetro de query 'code' (function key) en todas las peticiones HTTP (excepto OPTIONS)
    /// cuando se ha configurado Security:UrlKey. Permite mantener AuthorizationLevel.Anonymous sin perder control.
    /// </summary>
    public sealed class ApiKeyMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var req = await context.GetHttpRequestDataAsync();
            if (req == null)
            {
                await next(context);
                return;
            }

            var logger = context.GetLogger("ApiKeyMiddleware");

            // Preflight siempre pasa (CORS ya lo controla)
            if (string.Equals(req.Method, "OPTIONS", System.StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            var configuration = context.InstanceServices.GetService(typeof(IConfiguration)) as IConfiguration;
            var expected = configuration?["Security:UrlKey"] ?? configuration?["Values:Security:UrlKey"];
            if (!string.IsNullOrWhiteSpace(expected))
            {
                var query = req.Url.Query; // incluye '?'
                string? provided = null;
                if (!string.IsNullOrEmpty(query) && query.Length > 1)
                {
                    // Parse manual simple
                    var pairs = query.TrimStart('?').Split('&');
                    foreach (var p in pairs)
                    {
                        var kv = p.Split('=');
                        if (kv.Length == 2 && kv[0].Equals("code", System.StringComparison.OrdinalIgnoreCase))
                        {
                            provided = System.Web.HttpUtility.UrlDecode(kv[1]);
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(provided) || !string.Equals(provided, expected))
                {
                    logger.LogWarning("Solicitud bloqueada por ApiKeyMiddleware: code inválido o ausente para {Path}", req.Url.AbsolutePath);
                    var resp = req.CreateResponse(HttpStatusCode.Unauthorized);
                    CorsHelper.AddCorsHeaders(resp, req.Headers.TryGetValues("Origin", out var ov) ? ov.FirstOrDefault() : null, req.Headers.TryGetValues("Access-Control-Request-Headers", out var rh) ? rh.FirstOrDefault() : null);
                    await resp.WriteAsJsonAsync(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid or missing function key.",
                        Data = null,
                        StatusCode = StatusCodes.Status401Unauthorized
                    });
                    var invocation = context.GetInvocationResult();
                    invocation.Value = resp;
                    return;
                }
            }

            await next(context);
        }
    }
}

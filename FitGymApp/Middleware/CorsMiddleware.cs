using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using Gymmetry.Utils;

namespace FitGymApp.Middleware
{
    /// <summary>
    /// Middleware global CORS: añade encabezados a toda respuesta HTTP y maneja preflight
    /// si llegara a ejecutarse la canalización (fallback adicional al catch-all OPTIONS).
    /// </summary>
    public sealed class CorsMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var logger = context.GetLogger("CorsMiddleware");
            var req = await context.GetHttpRequestDataAsync();
            if (req == null)
            {
                await next(context);
                return;
            }

            string? origin = null;
            if (req.Headers.TryGetValues("Origin", out var originValues))
                origin = originValues.FirstOrDefault();
            string? requestedHeaders = null;
            if (req.Headers.TryGetValues("Access-Control-Request-Headers", out var acrh))
                requestedHeaders = acrh.FirstOrDefault();

            // No respondemos aquí al OPTIONS para permitir que el catch-all lo maneje primero.
            await next(context);

            try
            {
                var invocation = context.GetInvocationResult();
                if (invocation.Value is HttpResponseData resp)
                {
                    CorsHelper.AddCorsHeaders(resp, origin, requestedHeaders);
#if DEBUG
                    foreach (var h in resp.Headers)
                        logger.LogDebug("[CORS][MW] {Key}={Value}", h.Key, string.Join(';', h.Value));
#endif
                }
            }
            catch (System.Exception ex)
            {
                logger.LogWarning(ex, "Error agregando CORS en middleware");
            }
        }
    }
}

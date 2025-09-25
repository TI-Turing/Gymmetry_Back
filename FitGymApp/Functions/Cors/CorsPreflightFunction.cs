using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Gymmetry.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Functions.Cors
{
    /// <summary>
    /// Catch-all OPTIONS para preflight CORS. Evita editar cada HttpTrigger.
    /// </summary>
    public class CorsPreflightFunction
    {
        [Function("Cors_Preflight")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "options", Route = "{*any}")] HttpRequestData req,
            FunctionContext ctx)
        {
            var logger = ctx.GetLogger("Cors_Preflight");
            string? origin = null;
            if (req.Headers.TryGetValues("Origin", out var originValues))
                origin = originValues.FirstOrDefault();
            string? requestedHeaders = null;
            if (req.Headers.TryGetValues("Access-Control-Request-Headers", out var acrhValues))
                requestedHeaders = acrhValues.FirstOrDefault();

            var resp = req.CreateResponse(HttpStatusCode.NoContent);
            CorsHelper.AddCorsHeaders(resp, origin, requestedHeaders);
            logger.LogDebug("[CORS][Preflight] Origin={Origin} ReqHeaders={ReqHeaders}", origin, requestedHeaders);
            return await Task.FromResult(resp);
        }
    }
}

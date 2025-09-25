using Microsoft.Azure.Functions.Worker.Http;
using System.Linq;
using System.Collections.Generic;

namespace Gymmetry.Utils
{
    public static class CorsHelper
    {
        private static readonly string[] AllowedOrigins = new[]
        {
            "http://localhost:8081",
            "http://localhost:8082",
            "http://localhost:19006",
            "exp://localhost:8081",
            "exp://localhost:8082"
        };

        private static readonly string[] BaseAllowedHeaders = new[]
        {
            "Content-Type","Authorization","Accept","Cache-Control","Pragma","Expires","X-Requested-With","code","x-functions-key"
        };

        public static void AddCorsHeaders(HttpResponseData response, string? requestOrigin = null, string? requestedHeaders = null)
        {
            // Siempre reflejar Origin si llega, de lo contrario usar primero de la lista
            string origin = !string.IsNullOrWhiteSpace(requestOrigin) ? requestOrigin! : AllowedOrigins.First();
            if (response.Headers.Contains("Access-Control-Allow-Origin"))
                response.Headers.Remove("Access-Control-Allow-Origin");
            response.Headers.Add("Access-Control-Allow-Origin", origin);

            if (!response.Headers.Contains("Access-Control-Allow-Credentials"))
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            if (!response.Headers.Contains("Access-Control-Allow-Methods"))
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, PATCH, OPTIONS");

            var headers = new HashSet<string>(BaseAllowedHeaders, System.StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(requestedHeaders))
            {
                foreach (var h in requestedHeaders.Split(','))
                {
                    var trimmed = h.Trim();
                    if (!string.IsNullOrEmpty(trimmed)) headers.Add(trimmed);
                }
            }
            var allowHeadersValue = string.Join(", ", headers);
            if (response.Headers.Contains("Access-Control-Allow-Headers"))
                response.Headers.Remove("Access-Control-Allow-Headers");
            response.Headers.Add("Access-Control-Allow-Headers", allowHeadersValue);

            if (!response.Headers.Contains("Access-Control-Max-Age"))
                response.Headers.Add("Access-Control-Max-Age", "86400");
            if (!response.Headers.Contains("Vary"))
                response.Headers.Add("Vary", "Origin");
        }
    }
}

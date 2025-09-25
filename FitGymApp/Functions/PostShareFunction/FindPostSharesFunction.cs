using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.PostShare.Response;
using Gymmetry.Utils;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace Gymmetry.Functions.PostShareFunction
{
    public class FindPostSharesFunction
    {
        private readonly IPostShareService _postShareService;
        private readonly ILogger<FindPostSharesFunction> _logger;

        public FindPostSharesFunction(IPostShareService postShareService, ILogger<FindPostSharesFunction> logger)
        {
            _postShareService = postShareService;
            _logger = logger;
        }

        [Function("PostShare_FindPostSharesByFieldsFunction")]
        public async Task<HttpResponseData> FindByFieldsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "postShares/find")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("PostShare_FindPostSharesByFieldsFunction");
            logger.LogInformation("Procesando solicitud para buscar PostShares por filtros dinámicos.");

            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<PostShareResponse>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var filters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody);
                
                if (filters == null)
                {
                    filters = new Dictionary<string, object>();
                }

                // Convertir valores de filtros desde JsonElement a tipos apropiados
                var processedFilters = new Dictionary<string, object>();
                foreach (var kvp in filters)
                {
                    if (kvp.Value is JsonElement element)
                    {
                        switch (element.ValueKind)
                        {
                            case JsonValueKind.String:
                                var str = element.GetString();
                                // Intentar convertir strings que parecen GUIDs
                                if (Guid.TryParse(str, out var guid))
                                {
                                    processedFilters[kvp.Key] = guid;
                                }
                                // Intentar convertir strings que parecen fechas
                                else if (DateTime.TryParse(str, out var date))
                                {
                                    processedFilters[kvp.Key] = date;
                                }
                                else
                                {
                                    processedFilters[kvp.Key] = str ?? "";
                                }
                                break;
                            case JsonValueKind.True:
                                processedFilters[kvp.Key] = true;
                                break;
                            case JsonValueKind.False:
                                processedFilters[kvp.Key] = false;
                                break;
                            case JsonValueKind.Number:
                                processedFilters[kvp.Key] = element.GetInt32();
                                break;
                            default:
                                processedFilters[kvp.Key] = kvp.Value;
                                break;
                        }
                    }
                    else
                    {
                        processedFilters[kvp.Key] = kvp.Value;
                    }
                }

                var result = await _postShareService.FindPostSharesByFieldsAsync(processedFilters);

                // Asegurar que la respuesta sea compatible con el frontend (posible $values)
                var responseData = result.Data?.ToList();
                
                var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(new ApiResponse<IEnumerable<PostShareResponse>>
                {
                    Success = result.Success,
                    Message = result.Message,
                    Data = responseData,
                    StatusCode = result.Success ? StatusCodes.Status200OK : StatusCodes.Status400BadRequest
                });

                logger.LogInformation("PostShares encontrados exitosamente. Count: {Count}", responseData?.Count() ?? 0);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al buscar PostShares por filtros");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<PostShareResponse>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status500InternalServerError
                });
                return errorResponse;
            }
        }
    }
}
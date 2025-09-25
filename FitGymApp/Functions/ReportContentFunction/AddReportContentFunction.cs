using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ReportContent;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Gymmetry.Utils;

namespace Gymmetry.Functions.ReportContentFunction
{
    public class AddReportContentFunction
    {
        private readonly ILogger<AddReportContentFunction> _logger;
        private readonly IReportContentService _service;

        public AddReportContentFunction(ILogger<AddReportContentFunction> logger, IReportContentService service)
        {
            _logger = logger;
            _service = service;
        }

        [Function("ReportContent_AddReportContentFunction")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reportcontent")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ReportContent_AddReportContentFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<ReportContentResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            logger.LogInformation("Procesando solicitud de creación de reporte");
            
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var objRequest = JsonSerializer.Deserialize<ReportContentCreateRequest>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                var validation = ModelValidator.ValidateModel<ReportContentCreateRequest, ReportContentResponse>(objRequest, StatusCodes.Status400BadRequest);
                if (validation != null)
                {
                    var validationResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await validationResponse.WriteAsJsonAsync(validation);
                    return validationResponse;
                }

                if (userId.HasValue && objRequest!.ReporterId == Guid.Empty)
                {
                    objRequest.ReporterId = userId.Value;
                }

                var ip = FunctionResponseHelper.GetClientIp(req);
                var result = await _service.CreateAsync(objRequest!, userId, ip);
                
                if (!result.Success)
                {
                    var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await errorResponse.WriteAsJsonAsync(new ApiResponse<ReportContentResponse>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                    return errorResponse;
                }

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<ReportContentResponse>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data,
                    StatusCode = StatusCodes.Status200OK
                });
                return successResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear reporte");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<ReportContentResponse>
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
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ReportContent;
using Gymmetry.Repository.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Gymmetry.Utils;
using FitGymApp.Utils;

namespace Gymmetry.Functions.ReportContentFunction
{
    public class GetReportContentStatsFunction
    {
        private readonly ILogger<GetReportContentStatsFunction> _logger;
        private readonly IReportContentService _service;
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;

        public GetReportContentStatsFunction(ILogger<GetReportContentStatsFunction> logger, IReportContentService service, IUserRepository userRepository, IUserTypeRepository userTypeRepository)
        {
            _logger = logger;
            _service = service;
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
        }

        [Function("ReportContent_GetStatsFunction")]
        public async Task<HttpResponseData> GetStatsAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reportcontent/stats")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ReportContent_GetStatsFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<ReportContentStatsResponse>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
                return unauthorizedResponse;
            }

            var isModerator = await ModeratorValidator.IsModeratorAsync(userId, _userRepository, _userTypeRepository);
            if (!isModerator)
            {
                var forbiddenResponse = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<ReportContentStatsResponse>
                {
                    Success = false,
                    Message = "Acceso restringido",
                    Data = null,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation("Consultando estadísticas de ReportContent");
            
            try
            {
                var result = await _service.GetStatsAsync();

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<ReportContentStatsResponse>
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
                logger.LogError(ex, "Error al consultar estadísticas de ReportContent.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<ReportContentStatsResponse>
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
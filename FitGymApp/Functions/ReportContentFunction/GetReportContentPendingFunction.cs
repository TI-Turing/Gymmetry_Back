using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ReportContent;
using Gymmetry.Repository.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Utils;
using FitGymApp.Utils;

namespace Gymmetry.Functions.ReportContentFunction
{
    public class GetReportContentPendingFunction
    {
        private readonly ILogger<GetReportContentPendingFunction> _logger;
        private readonly IReportContentService _service;
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;

        public GetReportContentPendingFunction(ILogger<GetReportContentPendingFunction> logger, IReportContentService service, IUserRepository userRepository, IUserTypeRepository userTypeRepository)
        {
            _logger = logger;
            _service = service;
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
        }

        [Function("ReportContent_GetPendingFunction")]
        public async Task<HttpResponseData> GetPendingAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reportcontent/pending")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("ReportContent_GetPendingFunction");
            
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                var unauthorizedResponse = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<ReportContentResponse>>
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
                await forbiddenResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<ReportContentResponse>>
                {
                    Success = false,
                    Message = "Acceso restringido",
                    Data = null,
                    StatusCode = StatusCodes.Status403Forbidden
                });
                return forbiddenResponse;
            }

            logger.LogInformation("Consultando ReportContent pendientes");
            
            try
            {
                var result = await _service.GetPendingAsync();

                var successResponse = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<ReportContentResponse>>
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
                logger.LogError(ex, "Error al consultar ReportContent pendientes.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ApiResponse<IEnumerable<ReportContentResponse>>
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
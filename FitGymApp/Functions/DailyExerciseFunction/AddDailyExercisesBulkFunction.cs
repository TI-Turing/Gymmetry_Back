using Gymmetry.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.DailyExercise.Request;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FitGymApp.Functions.DailyExerciseFunction
{
    public class AddDailyExercisesBulkFunction
    {
        private readonly IDailyExerciseService _service;

        public AddDailyExercisesBulkFunction(IDailyExerciseService service)
        {
            _service = service;
        }

        [Function("DailyExercise_AddDailyExercisesBulkFunction")]
        public async Task<HttpResponseData> AddBulkAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "dailyexercises/addbulk")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("DailyExercise_AddDailyExercisesBulkFunction");
            if (!JwtValidator.ValidateJwt(req, out var error, out var userId))
            {
                return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.Unauthorized, new ApiResponse<IEnumerable<Guid>>
                {
                    Success = false,
                    Message = error!,
                    Data = null,
                    StatusCode = StatusCodes.Status401Unauthorized
                });
            }
            logger.LogInformation("Procesando solicitud para agregar ejercicios diarios masivamente.");
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var objRequest = JsonConvert.DeserializeObject<List<AddDailyExerciseRequest>>(requestBody);
                if (objRequest == null || !objRequest.Any())
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<IEnumerable<Guid>>
                    {
                        Success = false,
                        Message = "No se proporcionaron ejercicios válidos.",
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }
                var ip = FunctionResponseHelper.GetClientIp(req);
                foreach (var item in objRequest) item.Ip = ip;
                var result = await _service.CreateDailyExercisesBulkAsync(objRequest);
                if (!result.Success)
                {
                    return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<IEnumerable<Guid>>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = null,
                        StatusCode = StatusCodes.Status400BadRequest
                    });
                }
                return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.OK, new ApiResponse<IEnumerable<Guid>>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data?.Select(x => x.Id),
                    StatusCode = StatusCodes.Status200OK
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al agregar ejercicios diarios masivamente.");
                return await FunctionResponseHelper.CreateResponseAsync(req, HttpStatusCode.BadRequest, new ApiResponse<IEnumerable<Guid>>
                {
                    Success = false,
                    Message = "Ocurrió un error al procesar la solicitud.",
                    Data = null,
                    StatusCode = StatusCodes.Status400BadRequest
                });
            }
        }
    }
}

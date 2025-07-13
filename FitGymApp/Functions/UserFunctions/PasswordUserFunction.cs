using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO.User.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Functions.UserFunctions;

public class PasswordUserFunction
{
    private readonly ILogger<PasswordUserFunction> _logger;
    private readonly IUserService _userService;

    public PasswordUserFunction(ILogger<PasswordUserFunction> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [Function("User_PasswordUserFunction")]
    public async Task<HttpResponseData> UpdatePasswordAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "user/password/update")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("User_PasswordUserFunction");
        logger.LogInformation("Processing user password change request.");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var objRequest = JsonConvert.DeserializeObject<PasswordUserRequest>(requestBody);
        var result = await _userService.UpdatePasswordAsync(objRequest);
        var response = req.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
        await response.WriteAsJsonAsync(result);
        return response;
    }
}

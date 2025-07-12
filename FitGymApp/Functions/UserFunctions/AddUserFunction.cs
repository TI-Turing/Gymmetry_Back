using FitGymApp.Application.Services;
using FitGymApp.Application.Services.Interfaces;
using FitGymApp.Domain.DTO;
using FitGymApp.Domain.DTO.User.Request;
using FitGymApp.Domain.DTO.User.Response;
using FitGymApp.Domain.Models;
using FitGymApp.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FitGymApp.Functions.UserFunctions;

public class AddUserFunction
{
    private readonly ILogger<AddUserFunction> _logger;
    private readonly IUserService _userService;

    public AddUserFunction(ILogger<AddUserFunction> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [Function("User_AddUserFunction")]
    public async Task<ApiResponse<AddResponse>> AddAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "user/add")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var objRequest = JsonConvert.DeserializeObject<AddRequest>(requestBody);

            var validationResult = ModelValidator.ValidateModel<AddRequest, AddResponse>(objRequest, StatusCodes.Status400BadRequest);
            if (validationResult is not null) return validationResult;

            var result = _userService.CreateUser(objRequest);
            if (!result.Success)
            {
                return new ApiResponse<AddResponse>
                {
                    Success = false,
                    Message = result.Message,
                    Data = default,
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }
            var token = JwtTokenGenerator.GenerateToken(result.Data.Id, string.Empty, result.Data.Email);
            AddResponse addResponse = new AddResponse
            {
                Id = result.Data.Id,
                Token = token
            };
            return new ApiResponse<AddResponse>
            {
                Success = true,
                Message = result.Message,
                Data = result.Data != null ? addResponse : default,
                StatusCode = StatusCodes.Status200OK
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return new ApiResponse<AddResponse>
            {
                Success = false,
                Message = "Ocurrió un error al procesar la solicitud.",
                Data = default,
                StatusCode = StatusCodes.Status400BadRequest
            };
        }
    }
}
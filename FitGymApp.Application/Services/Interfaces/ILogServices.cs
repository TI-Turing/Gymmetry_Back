using FitGymApp.Domain.DTO.User.Response;
using System;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface ILogErrorService
    {
        ApplicationResponse<bool> LogError(Exception ex, string? userId = null, string? ip = null);
    }

    public interface ILogLoginService
    {
        ApplicationResponse<bool> LogLogin(Guid? userId, bool success, string? ip = null, string? message = null);
    }

    public interface ILogChangeService
    {
        ApplicationResponse<bool> LogChange(string table, object pastObject, Guid? userId, string? ip = null);
    }
}

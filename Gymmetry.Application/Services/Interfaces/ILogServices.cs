using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;
using System;
using System.Threading.Tasks;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface ILogErrorService
    {
        Task<ApplicationResponse<bool>> LogErrorAsync(Exception ex, string? userId = null, string? ip = null);
    }

    public interface ILogLoginService
    {
        Task<ApplicationResponse<bool>> LogLoginAsync(Guid? userId, bool success, string? ip = null, string? message = null);
        Task<ApplicationResponse<bool>> LogLoginAsync(Guid? userId, bool success, string refreshToken, DateTime refreshTokenExpiration, string? ip = null, string? message = null);
        Task<ApplicationResponse<LogLogin>> GetLogLoginByUserId(Guid userId);
        Task<ApplicationResponse<bool>> UpdateLogLoginAsync(LogLogin logLogin);
    }

    public interface ILogChangeService
    {
        Task<ApplicationResponse<bool>> LogChangeAsync(string table, object pastObject, Guid? userId, string? ip = null, string invocationId="");
        Task<ApplicationResponse<bool>> LogChangeAsync(string table, IEnumerable<object> pastObjects, Guid? userId, string? ip = null, string invocationId = "");
    }
}

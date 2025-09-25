using System;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.AppState;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IAppStateService
    {
        Task<ApplicationResponse<AppStateOverviewDto>> GetAppStateOverviewAsync(Guid userId);
    }
}
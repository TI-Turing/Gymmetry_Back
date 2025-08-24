using System.Threading.Tasks;
using Gymmetry.Domain.DTO.Analytics;
using Gymmetry.Domain.DTO;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<ApplicationResponse<AnalyticsSummaryResponse>> GetSummaryAsync(AnalyticsSummaryRequest request);
    }
}

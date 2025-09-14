using System.Threading.Tasks;
using Gymmetry.Domain.DTO.ReportContent;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IReportContentRealtimeService
    {
        Task ReportCreatedAsync(ReportContentResponse report);
        Task ReportUpdatedAsync(ReportContentResponse report);
        Task ReportReviewedAsync(ReportContentResponse report);
        Task StatsChangedAsync();
    }
}

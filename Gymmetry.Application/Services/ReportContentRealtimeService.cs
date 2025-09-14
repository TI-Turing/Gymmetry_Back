using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO.ReportContent;

namespace Gymmetry.Application.Services
{
    public class ReportContentRealtimeService : IReportContentRealtimeService
    {
        public Task ReportCreatedAsync(ReportContentResponse report) => Task.CompletedTask;
        public Task ReportUpdatedAsync(ReportContentResponse report) => Task.CompletedTask;
        public Task ReportReviewedAsync(ReportContentResponse report) => Task.CompletedTask;
        public Task StatsChangedAsync() => Task.CompletedTask;
    }
}

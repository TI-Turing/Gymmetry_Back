using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Progress;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IProgressReportService
    {
        Task<ApplicationResponse<ProgressSummaryResponse>> GetSummaryAsync(ProgressReportRequest request);
        Task<ApplicationResponse<ExercisesDetailResponse>> GetExercisesDetailAsync(ProgressReportRequest request);
        Task<ApplicationResponse<ObjectivesDetailResponse>> GetObjectivesDetailAsync(ProgressReportRequest request);
        Task<ApplicationResponse<MusclesDetailResponse>> GetMusclesDetailAsync(ProgressReportRequest request);
        Task<ApplicationResponse<SuggestionsResponse>> GetSuggestionsAsync(ProgressReportRequest request);
        Task<ApplicationResponse<DisciplineDetailResponse>> GetDisciplineDetailAsync(ProgressReportRequest request);
        Task<ApplicationResponse<List<ProgressSummaryResponse>>> GetMultiSummaryAsync(MultiProgressReportRequest request);
        Task<ApplicationResponse<MultiProgressHistoryResponse>> GetMultiSummaryWithHistoryAsync(MultiProgressReportRequest request);
    }
}

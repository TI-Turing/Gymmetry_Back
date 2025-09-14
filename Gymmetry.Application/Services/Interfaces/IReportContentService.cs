using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ReportContent;
using Gymmetry.Domain.Models;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IReportContentService
    {
        Task<ApplicationResponse<ReportContentResponse>> CreateAsync(ReportContentCreateRequest request, Guid? userId, string ip = "", string invocationId = "");
        Task<ApplicationResponse<ReportContentResponse>> GetByIdAsync(Guid id);
        Task<ApplicationResponse<ReportContentListResponse>> GetPagedAsync(int page, int pageSize);
        Task<ApplicationResponse<bool>> UpdateAsync(ReportContentUpdateRequest request, Guid? userId, string ip = "", string invocationId = "");
        Task<ApplicationResponse<bool>> DeleteAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<ReportContentResponse>>> FindByFieldsAsync(Dictionary<string, object> filters);
        Task<ApplicationResponse<IEnumerable<ReportContentResponse>>> GetPendingAsync();
        Task<ApplicationResponse<ReportContentResponse>> ReviewAsync(Guid id, Guid reviewerId, string? resolution, bool dismiss, bool resolve);
        Task<ApplicationResponse<ReportContentStatsResponse>> GetStatsAsync();
        Task<ApplicationResponse<IEnumerable<ReportContentResponse>>> GetByUserAsync(Guid userId);
        Task<ApplicationResponse<IEnumerable<ReportContentAudit>>> GetAuditsAsync(Guid reportId);
    }
}

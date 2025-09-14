using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ContentModeration;
using Gymmetry.Domain.Models;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IContentModerationService
    {
        Task<ApplicationResponse<ContentModerationResponse>> CreateAsync(ContentModerationCreateRequest request, Guid? userId, string ip = "", string invocationId = "");
        Task<ApplicationResponse<ContentModerationResponse>> GetByIdAsync(Guid id);
        Task<ApplicationResponse<ContentModerationListResponse>> GetPagedAsync(int page, int pageSize);
        Task<ApplicationResponse<bool>> UpdateAsync(ContentModerationUpdateRequest request, Guid? userId, string ip = "", string invocationId = "");
        Task<ApplicationResponse<bool>> DeleteAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<ContentModerationResponse>>> FindByFieldsAsync(Dictionary<string, object> filters);
        Task<ApplicationResponse<IEnumerable<ContentModerationResponse>>> GetPendingReviewAsync();
        Task<ApplicationResponse<ContentModerationStatsResponse>> GetStatsAsync();
        Task<ApplicationResponse<BulkModerationResponse>> BulkApproveAsync(BulkModerationRequest request, Guid moderatorId);
        Task<ApplicationResponse<BulkModerationResponse>> BulkRejectAsync(BulkModerationRequest request, Guid moderatorId);
        Task<ApplicationResponse<ContentModerationResponse>> GetByContentAsync(Guid contentId, ContentType contentType);
        Task<ApplicationResponse<AutoScanResponse>> AutoScanAsync(AutoScanRequest request);
    }
}
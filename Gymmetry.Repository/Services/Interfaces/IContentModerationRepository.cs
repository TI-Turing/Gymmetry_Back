using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IContentModerationRepository
    {
        Task<ContentModeration> CreateAsync(ContentModeration moderation);
        Task<ContentModeration?> GetByIdAsync(Guid id);
        Task<bool> UpdateAsync(ContentModeration moderation);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<ContentModeration>> GetPagedAsync(int page, int pageSize);
        Task<int> CountAsync();
        Task<IEnumerable<ContentModeration>> FindByFieldsAsync(Dictionary<string, object> filters);
        Task<IEnumerable<ContentModeration>> GetPendingReviewAsync();
        Task<ContentModeration?> GetByContentAsync(Guid contentId, ContentType contentType);
        Task<(int total, int auto, int manual, int pending)> GetStatsAsync();
        Task<Dictionary<string, int>> GetStatsByActionAsync();
        Task<Dictionary<string, int>> GetStatsByReasonAsync();
        Task<Dictionary<string, int>> GetStatsBySeverityAsync();
        Task<Dictionary<string, int>> GetStatsByContentTypeAsync();
        Task<Dictionary<string, int>> GetStatsLast7DaysAsync();
        Task<bool> BulkApproveAsync(IEnumerable<Guid> moderationIds, Guid moderatorId, string? notes);
        Task<bool> BulkRejectAsync(IEnumerable<Guid> moderationIds, Guid moderatorId, string? notes);
        Task<IEnumerable<ContentModeration>> GetContentForAutoScanAsync(ContentType? contentType, DateTime? sinceDate, int limit);
        Task ApplyModerationActionAsync(Guid contentId, ContentType contentType, ModerationAction action);
        Task<int> CountUserViolationsAsync(Guid userId, TimeSpan timeSpan);
    }
}
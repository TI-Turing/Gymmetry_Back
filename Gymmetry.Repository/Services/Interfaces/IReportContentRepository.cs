using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IReportContentRepository
    {
        Task<ReportContent> CreateAsync(ReportContent entity);
        Task<ReportContent?> GetByIdAsync(Guid id);
        Task<IEnumerable<ReportContent>> GetPagedAsync(int page,int pageSize);
        Task<int> CountAsync();
        Task<bool> UpdateAsync(ReportContent entity);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<ReportContent>> FindByFieldsAsync(Dictionary<string, object> filters);
        Task<IEnumerable<ReportContent>> GetPendingAsync();
        Task<ReportContent?> MarkReviewedAsync(Guid id, Guid reviewerId, string? resolution, bool dismiss, bool resolve);
        Task<(int total,int pending,int underReview,int resolved,int dismissed,Dictionary<string,int> byReason,Dictionary<string,int> byPriority,Dictionary<string,int> byType)> GetStatsAsync();
        Task<bool> ExistsDuplicateAsync(Guid reporterId, Guid reportedContentId, ReportContentType type);
        Task<int> CountForContentAsync(Guid reportedContentId, ReportContentType type);
        // Evidence
        Task<ReportContentEvidence> AddEvidenceAsync(ReportContentEvidence evidence);
        Task<List<ReportContentEvidence>> GetEvidenceAsync(Guid reportId);
        Task<bool> DeleteEvidenceAsync(Guid evidenceId, Guid reportId);
        // Audit
        Task<ReportContentAudit> AddAuditAsync(ReportContentAudit audit);
        Task<List<ReportContentAudit>> GetAuditsAsync(Guid reportId);
    }
}

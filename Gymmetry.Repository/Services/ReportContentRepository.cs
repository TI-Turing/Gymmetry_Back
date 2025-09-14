using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gymmetry.Repository.Services
{
    public class ReportContentRepository : IReportContentRepository
    {
        private readonly GymmetryContext _ctx;
        public ReportContentRepository(GymmetryContext ctx) { _ctx = ctx; }

        public async Task<ReportContent> CreateAsync(ReportContent entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsActive = true;
            _ctx.ReportContents.Add(entity);
            await _ctx.SaveChangesAsync();
            return entity;
        }

        public Task<ReportContent?> GetByIdAsync(Guid id) => _ctx.ReportContents.FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        public async Task<IEnumerable<ReportContent>> GetPagedAsync(int page,int pageSize)
        {
            if(page<1) page=1; if(pageSize<=0||pageSize>200) pageSize=50;
            return await _ctx.ReportContents.Where(r=>r.IsActive)
                .OrderByDescending(r=>r.CreatedAt)
                .Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
        }
        public Task<int> CountAsync()=>_ctx.ReportContents.CountAsync(r=>r.IsActive);

        public async Task<bool> UpdateAsync(ReportContent entity)
        {
            var existing = await _ctx.ReportContents.FirstOrDefaultAsync(r=>r.Id==entity.Id && r.IsActive);
            if(existing==null) return false;
            existing.Status = entity.Status;
            existing.Priority = entity.Priority;
            existing.ReviewedBy = entity.ReviewedBy;
            existing.ReviewedAt = entity.ReviewedAt;
            existing.Resolution = entity.Resolution;
            existing.UpdatedAt = DateTime.UtcNow;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _ctx.ReportContents.FirstOrDefaultAsync(r=>r.Id==id && r.IsActive);
            if(existing==null) return false;
            existing.IsActive=false; existing.DeletedAt=DateTime.UtcNow; await _ctx.SaveChangesAsync(); return true;
        }

        public async Task<IEnumerable<ReportContent>> FindByFieldsAsync(Dictionary<string, object> filters)
        {
            var param = Expression.Parameter(typeof(ReportContent),"r");
            Expression pred = Expression.Equal(Expression.Property(param,nameof(ReportContent.IsActive)), Expression.Constant(true));
            foreach(var f in filters){ var prop=typeof(ReportContent).GetProperty(f.Key); if(prop==null) continue; var left=Expression.Property(param,prop); var val=ValueConverter.ConvertValueToType(f.Value,prop.PropertyType); pred=Expression.AndAlso(pred, Expression.Equal(left, Expression.Constant(val))); }
            var lambda = Expression.Lambda<Func<ReportContent,bool>>(pred,param);
            return await _ctx.ReportContents.Where(lambda).OrderByDescending(r=>r.CreatedAt).ToListAsync();
        }

        public Task<IEnumerable<ReportContent>> GetPendingAsync()=> Task.FromResult<IEnumerable<ReportContent>>(_ctx.ReportContents.Where(r=>r.IsActive && r.Status==ReportStatus.Pending).OrderByDescending(r=>r.CreatedAt).Take(500).ToList());

        public async Task<ReportContent?> MarkReviewedAsync(Guid id, Guid reviewerId, string? resolution, bool dismiss, bool resolve)
        {
            var existing = await _ctx.ReportContents.FirstOrDefaultAsync(r=>r.Id==id && r.IsActive);
            if(existing==null) return null;
            existing.Status = resolve? ReportStatus.Resolved : dismiss? ReportStatus.Dismissed : ReportStatus.UnderReview;
            existing.ReviewedBy = reviewerId;
            existing.ReviewedAt = DateTime.UtcNow;
            if(!string.IsNullOrWhiteSpace(resolution)) existing.Resolution = resolution;
            existing.UpdatedAt = DateTime.UtcNow;
            await _ctx.SaveChangesAsync();
            return existing;
        }

        public async Task<(int total,int pending,int underReview,int resolved,int dismissed,Dictionary<string,int> byReason,Dictionary<string,int> byPriority,Dictionary<string,int> byType)> GetStatsAsync()
        {
            var q = _ctx.ReportContents.Where(r=>r.IsActive);
            var total = await q.CountAsync();
            var pending = await q.CountAsync(r=>r.Status==ReportStatus.Pending);
            var under = await q.CountAsync(r=>r.Status==ReportStatus.UnderReview);
            var res = await q.CountAsync(r=>r.Status==ReportStatus.Resolved);
            var dis = await q.CountAsync(r=>r.Status==ReportStatus.Dismissed);
            var byReason = await q.GroupBy(r=>r.Reason).Select(g=> new {k=g.Key, c=g.Count()}).ToDictionaryAsync(x=>x.k.ToString(),x=>x.c);
            var byPriority = await q.GroupBy(r=>r.Priority).Select(g=> new {k=g.Key, c=g.Count()}).ToDictionaryAsync(x=>x.k.ToString(),x=>x.c);
            var byType = await q.GroupBy(r=>r.ContentType).Select(g=> new {k=g.Key, c=g.Count()}).ToDictionaryAsync(x=>x.k.ToString(),x=>x.c);
            return (total,pending,under,res,dis,byReason,byPriority,byType);
        }

        public Task<bool> ExistsDuplicateAsync(Guid reporterId, Guid reportedContentId, ReportContentType type) => _ctx.ReportContents.AnyAsync(r=>r.IsActive && r.ReporterId==reporterId && r.ReportedContentId==reportedContentId && r.ContentType==type);

        public Task<int> CountForContentAsync(Guid reportedContentId, ReportContentType type)=> _ctx.ReportContents.CountAsync(r=>r.IsActive && r.ReportedContentId==reportedContentId && r.ContentType==type);

        public async Task<ReportContentEvidence> AddEvidenceAsync(ReportContentEvidence evidence)
        {
            evidence.Id = Guid.NewGuid();
            evidence.CreatedAt = DateTime.UtcNow;
            _ctx.ReportContentEvidences.Add(evidence);
            await _ctx.SaveChangesAsync();
            return evidence;
        }

        public Task<List<ReportContentEvidence>> GetEvidenceAsync(Guid reportId)
            => _ctx.ReportContentEvidences.Where(e=>e.ReportContentId==reportId).OrderByDescending(e=>e.CreatedAt).ToListAsync();

        public async Task<ReportContentAudit> AddAuditAsync(ReportContentAudit audit)
        {
            audit.Id = Guid.NewGuid();
            audit.CreatedAt = DateTime.UtcNow;
            _ctx.ReportContentAudits.Add(audit);
            await _ctx.SaveChangesAsync();
            return audit;
        }

        public Task<List<ReportContentAudit>> GetAuditsAsync(Guid reportId)
            => _ctx.ReportContentAudits.Where(a=>a.ReportContentId==reportId).OrderByDescending(a=>a.CreatedAt).ToListAsync();

        public async Task<bool> DeleteEvidenceAsync(Guid evidenceId, Guid reportId)
        {
            var evidence = await _ctx.ReportContentEvidences.FirstOrDefaultAsync(e => e.Id == evidenceId && e.ReportContentId == reportId);
            if (evidence == null) return false;
            _ctx.ReportContentEvidences.Remove(evidence);
            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}

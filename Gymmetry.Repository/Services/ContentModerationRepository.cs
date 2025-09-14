using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Gymmetry.Repository.Services.Cache;
using System.Text.Json;

namespace Gymmetry.Repository.Services
{
    public class ContentModerationRepository : IContentModerationRepository
    {
        private readonly GymmetryContext _context;
        private readonly ILogger<ContentModerationRepository> _logger;
        private readonly IRedisCacheService _redis;
        
        private const string ContentCachePrefix = "moderation:content:";
        private const string PendingCacheKey = "moderation:pending";
        private const string StatsCacheKey = "moderation:stats";
        private static readonly TimeSpan ContentCacheTtl = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan StatsCacheTtl = TimeSpan.FromMinutes(5);

        public ContentModerationRepository(GymmetryContext context, ILogger<ContentModerationRepository> logger, IRedisCacheService redis)
        {
            _context = context;
            _logger = logger;
            _redis = redis;
        }

        public async Task<ContentModeration> CreateAsync(ContentModeration moderation)
        {
            moderation.Id = Guid.NewGuid();
            moderation.CreatedAt = DateTime.UtcNow;
            moderation.ModeratedAt = DateTime.UtcNow;
            moderation.IsActive = true;

            _context.ContentModerations.Add(moderation);
            await _context.SaveChangesAsync();
            
            // Invalidar caches
            await InvalidateCachesAsync(moderation.ContentId, moderation.ContentType);
            
            // Aplicar acción de moderación al contenido
            await ApplyModerationActionAsync(moderation.ContentId, moderation.ContentType, moderation.ModerationAction);

            _logger.LogInformation("ContentModeration created: {Id} for content {ContentId}", moderation.Id, moderation.ContentId);
            return moderation;
        }

        public async Task<ContentModeration?> GetByIdAsync(Guid id)
        {
            return await _context.ContentModerations
                .Include(cm => cm.Moderator)
                .FirstOrDefaultAsync(cm => cm.Id == id && cm.IsActive);
        }

        public async Task<bool> UpdateAsync(ContentModeration moderation)
        {
            var existing = await _context.ContentModerations.FirstOrDefaultAsync(cm => cm.Id == moderation.Id && cm.IsActive);
            if (existing == null) return false;

            existing.ModerationAction = moderation.ModerationAction;
            existing.Severity = moderation.Severity;
            existing.ModeratedBy = moderation.ModeratedBy;
            existing.ReviewRequired = moderation.ReviewRequired;
            existing.Notes = moderation.Notes;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await InvalidateCachesAsync(existing.ContentId, existing.ContentType);
            
            // Aplicar nueva acción de moderación
            await ApplyModerationActionAsync(existing.ContentId, existing.ContentType, existing.ModerationAction);

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var moderation = await _context.ContentModerations.FirstOrDefaultAsync(cm => cm.Id == id && cm.IsActive);
            if (moderation == null) return false;

            moderation.IsActive = false;
            moderation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await InvalidateCachesAsync(moderation.ContentId, moderation.ContentType);

            return true;
        }

        public async Task<IEnumerable<ContentModeration>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.ContentModerations
                .Include(cm => cm.Moderator)
                .Where(cm => cm.IsActive)
                .OrderByDescending(cm => cm.ModeratedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.ContentModerations.CountAsync(cm => cm.IsActive);
        }

        public async Task<IEnumerable<ContentModeration>> FindByFieldsAsync(Dictionary<string, object> filters)
        {
            var query = _context.ContentModerations.Include(cm => cm.Moderator).AsQueryable();
            
            query = query.Where(cm => cm.IsActive);

            foreach (var filter in filters)
            {
                var property = typeof(ContentModeration).GetProperty(filter.Key);
                if (property != null)
                {
                    var parameter = Expression.Parameter(typeof(ContentModeration), "e");
                    var propertyAccess = Expression.Property(parameter, property);
                    var constant = Expression.Constant(filter.Value);
                    var equal = Expression.Equal(propertyAccess, constant);
                    var lambda = Expression.Lambda<Func<ContentModeration, bool>>(equal, parameter);
                    
                    query = query.Where(lambda);
                }
            }

            return await query.OrderByDescending(cm => cm.ModeratedAt).ToListAsync();
        }

        public async Task<IEnumerable<ContentModeration>> GetPendingReviewAsync()
        {
            try
            {
                var cached = await _redis.GetAsync(PendingCacheKey);
                if (cached != null)
                {
                    var cachedList = JsonSerializer.Deserialize<List<ContentModeration>>(cached);
                    if (cachedList != null) return cachedList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting cache for pending moderations");
            }

            var result = await _context.ContentModerations
                .Include(cm => cm.Moderator)
                .Where(cm => cm.IsActive && cm.ReviewRequired)
                .OrderBy(cm => cm.ModeratedAt)
                .ToListAsync();

            try
            {
                await _redis.SetAsync(PendingCacheKey, JsonSerializer.Serialize(result), StatsCacheTtl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting cache for pending moderations");
            }

            return result;
        }

        public async Task<ContentModeration?> GetByContentAsync(Guid contentId, ContentType contentType)
        {
            var cacheKey = $"{ContentCachePrefix}{contentId}:{(int)contentType}";
            
            try
            {
                var cached = await _redis.GetAsync(cacheKey);
                if (cached != null)
                {
                    var cachedItem = JsonSerializer.Deserialize<ContentModeration>(cached);
                    if (cachedItem != null) return cachedItem;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting cache for content moderation");
            }

            var result = await _context.ContentModerations
                .Include(cm => cm.Moderator)
                .Where(cm => cm.ContentId == contentId && cm.ContentType == contentType && cm.IsActive)
                .OrderByDescending(cm => cm.ModeratedAt)
                .FirstOrDefaultAsync();

            if (result != null)
            {
                try
                {
                    await _redis.SetAsync(cacheKey, JsonSerializer.Serialize(result), ContentCacheTtl);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error setting cache for content moderation");
                }
            }

            return result;
        }

        public async Task<(int total, int auto, int manual, int pending)> GetStatsAsync()
        {
            var total = await _context.ContentModerations.CountAsync(cm => cm.IsActive);
            var auto = await _context.ContentModerations.CountAsync(cm => cm.IsActive && cm.AutoModerated);
            var manual = await _context.ContentModerations.CountAsync(cm => cm.IsActive && !cm.AutoModerated);
            var pending = await _context.ContentModerations.CountAsync(cm => cm.IsActive && cm.ReviewRequired);

            return (total, auto, manual, pending);
        }

        public async Task<Dictionary<string, int>> GetStatsByActionAsync()
        {
            var stats = await _context.ContentModerations
                .Where(cm => cm.IsActive)
                .GroupBy(cm => cm.ModerationAction)
                .Select(g => new { Action = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            return stats.ToDictionary(s => s.Action, s => s.Count);
        }

        public async Task<Dictionary<string, int>> GetStatsByReasonAsync()
        {
            var stats = await _context.ContentModerations
                .Where(cm => cm.IsActive)
                .GroupBy(cm => cm.ModerationReason)
                .Select(g => new { Reason = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            return stats.ToDictionary(s => s.Reason, s => s.Count);
        }

        public async Task<Dictionary<string, int>> GetStatsBySeverityAsync()
        {
            var stats = await _context.ContentModerations
                .Where(cm => cm.IsActive)
                .GroupBy(cm => cm.Severity)
                .Select(g => new { Severity = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            return stats.ToDictionary(s => s.Severity, s => s.Count);
        }

        public async Task<Dictionary<string, int>> GetStatsByContentTypeAsync()
        {
            var stats = await _context.ContentModerations
                .Where(cm => cm.IsActive)
                .GroupBy(cm => cm.ContentType)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            return stats.ToDictionary(s => s.Type, s => s.Count);
        }

        public async Task<Dictionary<string, int>> GetStatsLast7DaysAsync()
        {
            var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-7);
            
            var stats = await _context.ContentModerations
                .Where(cm => cm.IsActive && cm.ModeratedAt >= sevenDaysAgo)
                .GroupBy(cm => cm.ModeratedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            var result = new Dictionary<string, int>();
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                var dateString = date.ToString("yyyy-MM-dd");
                result[dateString] = stats.FirstOrDefault(s => s.Date == date)?.Count ?? 0;
            }

            return result;
        }

        public async Task<bool> BulkApproveAsync(IEnumerable<Guid> moderationIds, Guid moderatorId, string? notes)
        {
            var moderations = await _context.ContentModerations
                .Where(cm => moderationIds.Contains(cm.Id) && cm.IsActive && cm.ReviewRequired)
                .ToListAsync();

            if (!moderations.Any()) return false;

            var now = DateTime.UtcNow;
            foreach (var moderation in moderations)
            {
                moderation.ReviewRequired = false;
                moderation.ModeratedBy = moderatorId;
                moderation.Notes = notes;
                moderation.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();
            await InvalidateCachesAsync();

            return true;
        }

        public async Task<bool> BulkRejectAsync(IEnumerable<Guid> moderationIds, Guid moderatorId, string? notes)
        {
            var moderations = await _context.ContentModerations
                .Where(cm => moderationIds.Contains(cm.Id) && cm.IsActive && cm.ReviewRequired)
                .ToListAsync();

            if (!moderations.Any()) return false;

            var now = DateTime.UtcNow;
            foreach (var moderation in moderations)
            {
                moderation.ModerationAction = ModerationAction.NoAction;
                moderation.ReviewRequired = false;
                moderation.ModeratedBy = moderatorId;
                moderation.Notes = notes;
                moderation.UpdatedAt = now;
                
                // Revertir acción de moderación
                await ApplyModerationActionAsync(moderation.ContentId, moderation.ContentType, ModerationAction.NoAction);
            }

            await _context.SaveChangesAsync();
            await InvalidateCachesAsync();

            return true;
        }

        public async Task<IEnumerable<ContentModeration>> GetContentForAutoScanAsync(ContentType? contentType, DateTime? sinceDate, int limit)
        {
            var query = _context.ContentModerations.AsQueryable();

            if (contentType.HasValue)
                query = query.Where(cm => cm.ContentType == contentType.Value);

            if (sinceDate.HasValue)
                query = query.Where(cm => cm.CreatedAt >= sinceDate.Value);

            return await query
                .Where(cm => cm.IsActive)
                .OrderByDescending(cm => cm.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task ApplyModerationActionAsync(Guid contentId, ContentType contentType, ModerationAction action)
        {
            try
            {
                switch (contentType)
                {
                    case ContentType.Feed:
                        await ApplyFeedModerationAsync(contentId, action);
                        break;
                    case ContentType.Comment:
                        await ApplyCommentModerationAsync(contentId, action);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying moderation action {Action} to {ContentType} {ContentId}", action, contentType, contentId);
            }
        }

        public async Task<int> CountUserViolationsAsync(Guid userId, TimeSpan timeSpan)
        {
            var cutoff = DateTime.UtcNow.Subtract(timeSpan);
            
            // Contar moderaciones donde el usuario es el autor del contenido
            var feedViolations = await _context.ContentModerations
                .Where(cm => cm.IsActive && 
                           cm.ContentType == ContentType.Feed && 
                           cm.ModeratedAt >= cutoff &&
                           cm.ModerationAction != ModerationAction.NoAction)
                .Join(_context.Feeds, cm => cm.ContentId, f => f.Id, (cm, f) => f.UserId)
                .CountAsync(userId => userId == userId);

            var commentViolations = await _context.ContentModerations
                .Where(cm => cm.IsActive && 
                           cm.ContentType == ContentType.Comment && 
                           cm.ModeratedAt >= cutoff &&
                           cm.ModerationAction != ModerationAction.NoAction)
                .Join(_context.FeedComments, cm => cm.ContentId, fc => fc.Id, (cm, fc) => fc.UserId)
                .CountAsync(userId => userId == userId);

            return feedViolations + commentViolations;
        }

        private async Task ApplyFeedModerationAsync(Guid feedId, ModerationAction action)
        {
            var feed = await _context.Feeds.FirstOrDefaultAsync(f => f.Id == feedId);
            if (feed == null) return;

            switch (action)
            {
                case ModerationAction.Hidden:
                    feed.IsActive = false; // Ocultar en feeds
                    break;
                case ModerationAction.Removed:
                    feed.IsActive = false;
                    feed.DeletedAt = DateTime.UtcNow;
                    break;
                case ModerationAction.NoAction:
                    feed.IsActive = true; // Restaurar si fue revertido
                    feed.DeletedAt = null;
                    break;
            }

            await _context.SaveChangesAsync();
        }

        private async Task ApplyCommentModerationAsync(Guid commentId, ModerationAction action)
        {
            var comment = await _context.FeedComments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) return;

            switch (action)
            {
                case ModerationAction.Hidden:
                    comment.IsActive = false; // Ocultar comentario
                    break;
                case ModerationAction.Removed:
                    comment.IsActive = false;
                    comment.DeletedAt = DateTime.UtcNow;
                    break;
                case ModerationAction.NoAction:
                    comment.IsActive = true; // Restaurar si fue revertido
                    comment.DeletedAt = null;
                    break;
            }

            await _context.SaveChangesAsync();
        }

        private async Task InvalidateCachesAsync(Guid? contentId = null, ContentType? contentType = null)
        {
            try
            {
                await _redis.RemoveAsync(PendingCacheKey);
                await _redis.RemoveAsync(StatsCacheKey);

                if (contentId.HasValue && contentType.HasValue)
                {
                    var cacheKey = $"{ContentCachePrefix}{contentId}:{(int)contentType}";
                    await _redis.RemoveAsync(cacheKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error invalidating caches");
            }
        }
    }
}
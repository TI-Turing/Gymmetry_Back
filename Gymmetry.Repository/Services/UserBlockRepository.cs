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
    public class UserBlockRepository : IUserBlockRepository
    {
        private readonly GymmetryContext _context;
        private readonly ILogger<UserBlockRepository> _logger;
        private readonly IRedisCacheService _redis;
        
        private const string BlockedUsersCachePrefix = "userblocks:blocked:";
        private const string BlockersCachePrefix = "userblocks:blockers:";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

        public UserBlockRepository(GymmetryContext context, ILogger<UserBlockRepository> logger, IRedisCacheService redis)
        {
            _context = context;
            _logger = logger;
            _redis = redis;
        }

        public async Task<UserBlock> CreateAsync(UserBlock userBlock)
        {
            userBlock.Id = Guid.NewGuid();
            userBlock.CreatedAt = DateTime.UtcNow;
            userBlock.IsActive = true;

            _context.UserBlocks.Add(userBlock);
            await _context.SaveChangesAsync();
            
            // Invalidar cache
            await InvalidateCacheAsync(userBlock.BlockerId, userBlock.BlockedUserId);
            
            // Cleanup interacciones existentes
            await CleanupInteractionsAsync(userBlock.BlockerId, userBlock.BlockedUserId);

            _logger.LogInformation("User {BlockerId} blocked user {BlockedUserId}", userBlock.BlockerId, userBlock.BlockedUserId);
            return userBlock;
        }

        public async Task<UserBlock?> GetByIdAsync(Guid id)
        {
            return await _context.UserBlocks
                .Include(ub => ub.Blocker)
                .Include(ub => ub.BlockedUser)
                .FirstOrDefaultAsync(ub => ub.Id == id && ub.IsActive);
        }

        public async Task<bool> DeleteAsync(Guid blockerId, Guid blockedUserId)
        {
            var userBlock = await _context.UserBlocks
                .FirstOrDefaultAsync(ub => ub.BlockerId == blockerId && ub.BlockedUserId == blockedUserId && ub.IsActive);
            
            if (userBlock == null) return false;

            // Soft delete
            userBlock.IsActive = false;
            userBlock.DeletedAt = DateTime.UtcNow;
            userBlock.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await InvalidateCacheAsync(blockerId, blockedUserId);

            _logger.LogInformation("User {BlockerId} unblocked user {BlockedUserId}", blockerId, blockedUserId);
            return true;
        }

        public async Task<bool> ExistsAsync(Guid blockerId, Guid blockedUserId)
        {
            return await _context.UserBlocks
                .AnyAsync(ub => ub.BlockerId == blockerId && ub.BlockedUserId == blockedUserId && ub.IsActive);
        }

        public async Task<IEnumerable<UserBlock>> GetBlockedByUserAsync(Guid blockerId, int page = 1, int pageSize = 50)
        {
            var cacheKey = $"{BlockedUsersCachePrefix}{blockerId}:page:{page}:size:{pageSize}";
            
            try
            {
                var cached = await _redis.GetAsync(cacheKey);
                if (cached != null)
                {
                    var cachedList = JsonSerializer.Deserialize<List<UserBlock>>(cached);
                    if (cachedList != null) return cachedList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting cache for blocked users");
            }

            var result = await _context.UserBlocks
                .Include(ub => ub.BlockedUser)
                .Where(ub => ub.BlockerId == blockerId && ub.IsActive)
                .OrderByDescending(ub => ub.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            try
            {
                await _redis.SetAsync(cacheKey, JsonSerializer.Serialize(result), CacheTtl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting cache for blocked users");
            }

            return result;
        }

        public async Task<IEnumerable<UserBlock>> GetBlockersOfUserAsync(Guid blockedUserId, int page = 1, int pageSize = 50)
        {
            var cacheKey = $"{BlockersCachePrefix}{blockedUserId}:page:{page}:size:{pageSize}";
            
            try
            {
                var cached = await _redis.GetAsync(cacheKey);
                if (cached != null)
                {
                    var cachedList = JsonSerializer.Deserialize<List<UserBlock>>(cached);
                    if (cachedList != null) return cachedList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting cache for blockers");
            }

            var result = await _context.UserBlocks
                .Include(ub => ub.Blocker)
                .Where(ub => ub.BlockedUserId == blockedUserId && ub.IsActive)
                .OrderByDescending(ub => ub.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            try
            {
                await _redis.SetAsync(cacheKey, JsonSerializer.Serialize(result), CacheTtl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting cache for blockers");
            }

            return result;
        }

        public async Task<IEnumerable<UserBlock>> GetMutualBlocksAsync(Guid userId, int page = 1, int pageSize = 50)
        {
            var blockedByMe = await _context.UserBlocks
                .Where(ub => ub.BlockerId == userId && ub.IsActive)
                .Select(ub => ub.BlockedUserId)
                .ToListAsync();

            return await _context.UserBlocks
                .Include(ub => ub.BlockedUser)
                .Where(ub => ub.BlockedUserId == userId && ub.IsActive && blockedByMe.Contains(ub.BlockerId))
                .OrderByDescending(ub => ub.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserBlock>> FindByFieldsAsync(Dictionary<string, object> filters)
        {
            var query = _context.UserBlocks.Include(ub => ub.Blocker).Include(ub => ub.BlockedUser).AsQueryable();
            
            query = query.Where(ub => ub.IsActive);

            foreach (var filter in filters)
            {
                var property = typeof(UserBlock).GetProperty(filter.Key);
                if (property != null)
                {
                    var parameter = Expression.Parameter(typeof(UserBlock), "e");
                    var propertyAccess = Expression.Property(parameter, property);
                    var constant = Expression.Constant(filter.Value);
                    var equal = Expression.Equal(propertyAccess, constant);
                    var lambda = Expression.Lambda<Func<UserBlock, bool>>(equal, parameter);
                    
                    query = query.Where(lambda);
                }
            }

            return await query.OrderByDescending(ub => ub.CreatedAt).ToListAsync();
        }

        public async Task<UserBlock?> GetBlockBetweenUsersAsync(Guid user1Id, Guid user2Id)
        {
            return await _context.UserBlocks
                .Include(ub => ub.Blocker)
                .Include(ub => ub.BlockedUser)
                .FirstOrDefaultAsync(ub => 
                    ((ub.BlockerId == user1Id && ub.BlockedUserId == user2Id) ||
                     (ub.BlockerId == user2Id && ub.BlockedUserId == user1Id)) && 
                    ub.IsActive);
        }

        public async Task<int> CountBlocksByUserTodayAsync(Guid blockerId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            
            return await _context.UserBlocks
                .CountAsync(ub => ub.BlockerId == blockerId && 
                                 ub.CreatedAt >= today && 
                                 ub.CreatedAt < tomorrow && 
                                 ub.IsActive);
        }

        public async Task<int> CountTotalBlocksAsync()
        {
            return await _context.UserBlocks.CountAsync(ub => ub.IsActive);
        }

        public async Task<int> CountBlockedByUserAsync(Guid blockerId)
        {
            return await _context.UserBlocks.CountAsync(ub => ub.BlockerId == blockerId && ub.IsActive);
        }

        public async Task<int> CountBlockersOfUserAsync(Guid blockedUserId)
        {
            return await _context.UserBlocks.CountAsync(ub => ub.BlockedUserId == blockedUserId && ub.IsActive);
        }

        public async Task<int> CountMutualBlocksAsync(Guid userId)
        {
            var blockedByMe = await _context.UserBlocks
                .Where(ub => ub.BlockerId == userId && ub.IsActive)
                .Select(ub => ub.BlockedUserId)
                .ToListAsync();

            return await _context.UserBlocks
                .CountAsync(ub => ub.BlockedUserId == userId && ub.IsActive && blockedByMe.Contains(ub.BlockerId));
        }

        public async Task<Dictionary<string, int>> GetBlocksStatsLast7DaysAsync(Guid userId)
        {
            var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-7);
            
            var blocks = await _context.UserBlocks
                .Where(ub => ub.BlockerId == userId && ub.CreatedAt >= sevenDaysAgo && ub.IsActive)
                .GroupBy(ub => ub.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Date)
                .ToListAsync();

            var result = new Dictionary<string, int>();
            for (int i = 0; i < 7; i++)
            {
                var date = DateTime.UtcNow.Date.AddDays(-i);
                var dateString = date.ToString("yyyy-MM-dd");
                result[dateString] = blocks.FirstOrDefault(b => b.Date == date)?.Count ?? 0;
            }

            return result;
        }

        public async Task<bool> BulkUnblockAsync(Guid blockerId, IEnumerable<Guid> blockedUserIds)
        {
            var userBlocks = await _context.UserBlocks
                .Where(ub => ub.BlockerId == blockerId && 
                            blockedUserIds.Contains(ub.BlockedUserId) && 
                            ub.IsActive)
                .ToListAsync();

            if (!userBlocks.Any()) return false;

            var now = DateTime.UtcNow;
            foreach (var userBlock in userBlocks)
            {
                userBlock.IsActive = false;
                userBlock.DeletedAt = now;
                userBlock.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            // Invalidar cache para todos los usuarios afectados
            foreach (var blockedUserId in blockedUserIds)
            {
                await InvalidateCacheAsync(blockerId, blockedUserId);
            }

            _logger.LogInformation("User {BlockerId} unblocked {Count} users in bulk", blockerId, userBlocks.Count);
            return true;
        }

        public async Task CleanupInteractionsAsync(Guid blockerId, Guid blockedUserId)
        {
            try
            {
                // Eliminar likes entre usuarios bloqueados
                var likesToRemove = await _context.Likes
                    .Where(l => (l.UserId == blockerId && l.Post.UserId == blockedUserId) ||
                               (l.UserId == blockedUserId && l.Post.UserId == blockerId))
                    .ToListAsync();

                _context.Likes.RemoveRange(likesToRemove);

                var feedLikesToRemove = await _context.FeedLikes
                    .Where(fl => (fl.UserId == blockerId && fl.Feed.UserId == blockedUserId) ||
                                (fl.UserId == blockedUserId && fl.Feed.UserId == blockerId))
                    .ToListAsync();

                _context.FeedLikes.RemoveRange(feedLikesToRemove);

                // Soft delete comments entre usuarios bloqueados
                var commentsToRemove = await _context.Comments
                    .Where(c => (c.UserId == blockerId && c.Post.UserId == blockedUserId) ||
                               (c.UserId == blockedUserId && c.Post.UserId == blockerId))
                    .ToListAsync();

                foreach (var comment in commentsToRemove)
                {
                    comment.IsActive = false;
                    comment.DeletedAt = DateTime.UtcNow;
                }

                var feedCommentsToRemove = await _context.FeedComments
                    .Where(fc => (fc.UserId == blockerId && fc.Feed.UserId == blockedUserId) ||
                                (fc.UserId == blockedUserId && fc.Feed.UserId == blockerId))
                    .ToListAsync();

                foreach (var feedComment in feedCommentsToRemove)
                {
                    feedComment.IsActive = false;
                    feedComment.DeletedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up interactions between users {BlockerId} and {BlockedUserId}", blockerId, blockedUserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up interactions between users {BlockerId} and {BlockedUserId}", blockerId, blockedUserId);
            }
        }

        public async Task<bool> IsUserBlockedByMeAsync(Guid myUserId, Guid targetUserId)
        {
            return await _context.UserBlocks
                .AnyAsync(ub => ub.BlockerId == myUserId && ub.BlockedUserId == targetUserId && ub.IsActive);
        }

        public async Task<bool> AmIBlockedByUserAsync(Guid myUserId, Guid targetUserId)
        {
            return await _context.UserBlocks
                .AnyAsync(ub => ub.BlockerId == targetUserId && ub.BlockedUserId == myUserId && ub.IsActive);
        }

        public async Task<IEnumerable<Guid>> GetBlockedUserIdsAsync(Guid blockerId)
        {
            var cacheKey = $"{BlockedUsersCachePrefix}{blockerId}:ids";
            
            try
            {
                var cached = await _redis.GetAsync(cacheKey);
                if (cached != null)
                {
                    var cachedList = JsonSerializer.Deserialize<List<Guid>>(cached);
                    if (cachedList != null) return cachedList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting cache for blocked user IDs");
            }

            var result = await _context.UserBlocks
                .Where(ub => ub.BlockerId == blockerId && ub.IsActive)
                .Select(ub => ub.BlockedUserId)
                .ToListAsync();

            try
            {
                await _redis.SetAsync(cacheKey, JsonSerializer.Serialize(result), CacheTtl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting cache for blocked user IDs");
            }

            return result;
        }

        public async Task<IEnumerable<Guid>> GetBlockerUserIdsAsync(Guid blockedUserId)
        {
            var cacheKey = $"{BlockersCachePrefix}{blockedUserId}:ids";
            
            try
            {
                var cached = await _redis.GetAsync(cacheKey);
                if (cached != null)
                {
                    var cachedList = JsonSerializer.Deserialize<List<Guid>>(cached);
                    if (cachedList != null) return cachedList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting cache for blocker user IDs");
            }

            var result = await _context.UserBlocks
                .Where(ub => ub.BlockedUserId == blockedUserId && ub.IsActive)
                .Select(ub => ub.BlockerId)
                .ToListAsync();

            try
            {
                await _redis.SetAsync(cacheKey, JsonSerializer.Serialize(result), CacheTtl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting cache for blocker user IDs");
            }

            return result;
        }

        private async Task InvalidateCacheAsync(Guid blockerId, Guid blockedUserId)
        {
            try
            {
                var keysToRemove = new[]
                {
                    $"{BlockedUsersCachePrefix}{blockerId}:ids",
                    $"{BlockersCachePrefix}{blockedUserId}:ids"
                };

                foreach (var key in keysToRemove)
                {
                    await _redis.RemoveAsync(key);
                }

                // También limpiar las claves de paginación (aproximación)
                for (int page = 1; page <= 10; page++)
                {
                    for (int size = 10; size <= 100; size += 10)
                    {
                        await _redis.RemoveAsync($"{BlockedUsersCachePrefix}{blockerId}:page:{page}:size:{size}");
                        await _redis.RemoveAsync($"{BlockersCachePrefix}{blockedUserId}:page:{page}:size:{size}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error invalidating cache for users {BlockerId} and {BlockedUserId}", blockerId, blockedUserId);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Gymmetry.Domain.DTO.Feed.Request;
using Gymmetry.Repository.Services.Cache;

namespace Gymmetry.Repository.Services
{
    public class FeedRepository : IFeedRepository
    {
        private readonly GymmetryContext _context;
        private readonly IConfiguration _configuration;
        private readonly IRedisCacheService _redisCache;
        private readonly ILogger<FeedRepository> _logger;
        private const string RedisPrefix = "feed:";

        public FeedRepository(GymmetryContext context, IConfiguration configuration, IRedisCacheService redisCache, ILogger<FeedRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _redisCache = redisCache ?? throw new ArgumentNullException(nameof(redisCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // NEW: Multimedia feed creation with transactional support
        public async Task<Feed> CreateFeedWithMediaAsync(Feed feed, List<FeedMedia> mediaFiles)
        {
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            
            try
            {
                // Start a database transaction to ensure consistency
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Ensure proper IDs and timestamps
                    feed.Id = Guid.NewGuid();
                    feed.CreatedAt = DateTime.UtcNow;
                    feed.UpdatedAt = DateTime.UtcNow;
                    feed.IsActive = true;
                    feed.IsDeleted = false;

                    // 1. Add the Feed entity
                    _context.Feeds.Add(feed);
                    await _context.SaveChangesAsync();

                    // 2. Add the FeedMedia entities if any
                    if (mediaFiles != null && mediaFiles.Any())
                    {
                        // Ensure all FeedMedia have the correct FeedId and proper IDs
                        foreach (var mediaFile in mediaFiles)
                        {
                            mediaFile.Id = Guid.NewGuid();
                            mediaFile.FeedId = feed.Id;
                            mediaFile.CreatedAt = DateTime.UtcNow;
                            mediaFile.IsActive = true;
                        }

                        _context.FeedMedia.AddRange(mediaFiles);
                        await _context.SaveChangesAsync();
                    }

                    // 3. Commit the transaction
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully created feed {FeedId} with {MediaCount} media files", 
                        feed.Id, mediaFiles?.Count ?? 0);

                    return feed;
                }
                catch (Exception)
                {
                    // Rollback the transaction on error
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feed with media for FeedId: {FeedId}", feed.Id);
                throw;
            }
        }

        public async Task<Feed> AddFeedAsync(Feed feed, byte[]? media = null, string? fileName = null, string? mediaType = null)
        {
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            try
            {
                feed.Id = Guid.NewGuid();
                feed.CreatedAt = DateTime.UtcNow;
                feed.IsActive = true;
                feed.IsDeleted = false;

                if (media != null && !string.IsNullOrEmpty(fileName))
                {
                    feed.MediaUrl = await UploadFeedMediaAsync(feed.Id, media, fileName, mediaType).ConfigureAwait(false);
                    feed.MediaType = mediaType;
                }

                _context.Feeds.Add(feed);
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return feed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding feed");
                throw;
            }
        }

        public async Task<bool> UpdateFeedAsync(Feed feed, byte[]? media = null, string? fileName = null, string? mediaType = null)
        {
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            try
            {
                var existing = await _context.Feeds.FirstOrDefaultAsync(f => f.Id == feed.Id && f.IsActive && !f.IsDeleted).ConfigureAwait(false);
                if (existing == null) return false;

                existing.Title = feed.Title ?? existing.Title;
                existing.Description = feed.Description ?? existing.Description;
                existing.UpdatedAt = DateTime.UtcNow;
                if (media != null && !string.IsNullOrEmpty(fileName))
                {
                    existing.MediaUrl = await UploadFeedMediaAsync(existing.Id, media, fileName, mediaType).ConfigureAwait(false);
                    existing.MediaType = mediaType;
                }
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feed");
                throw;
            }
        }

        public async Task<bool> DeleteFeedAsync(Guid feedId)
        {
            try
            {
                var feed = await _context.Feeds.FirstOrDefaultAsync(f => f.Id == feedId && f.IsActive && !f.IsDeleted).ConfigureAwait(false);
                if (feed == null) return false;
                feed.IsDeleted = true;
                feed.IsActive = false;
                feed.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feed");
                throw;
            }
        }

        public async Task<Feed?> GetFeedByIdAsync(Guid feedId)
        {
            string cacheKey = RedisPrefix + feedId;
            try
            {
                var cached = await _redisCache.GetAsync(cacheKey).ConfigureAwait(false);
                if (cached != null)
                {
                    return System.Text.Json.JsonSerializer.Deserialize<Feed>(cached);
                }
                var feed = await _context.Feeds.FirstOrDefaultAsync(f => f.Id == feedId && f.IsActive && !f.IsDeleted).ConfigureAwait(false);
                if (feed != null)
                {
                    await _redisCache.SetAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(feed), TimeSpan.FromMinutes(10)).ConfigureAwait(false);
                }
                return feed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feed by id {FeedId}", feedId);
                throw;
            }
        }

        public async Task<IEnumerable<Feed>> GetFeedsByUserAsync(Guid userId)
        {
            string cacheKey = $"user:feeds:{userId}";
            try
            {
                var cached = await _redisCache.GetAsync(cacheKey).ConfigureAwait(false);
                if (cached != null)
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<Feed>>(cached) ?? new List<Feed>();
                }
                var feeds = await _context.Feeds.Where(f => f.UserId == userId && f.IsActive && !f.IsDeleted)
                    .OrderByDescending(f => f.CreatedAt).ToListAsync().ConfigureAwait(false);
                if (feeds.Any())
                {
                    await _redisCache.SetAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(feeds), TimeSpan.FromMinutes(5)).ConfigureAwait(false);
                }
                return feeds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feeds by user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<Feed>> GetAllFeedsAsync()
        {
            try
            {
                return await _context.Feeds.Where(f => f.IsActive && !f.IsDeleted)
                    .OrderByDescending(f => f.CreatedAt).ToListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all feeds");
                throw;
            }
        }

        public async Task<string> UploadFeedMediaToBlobAsync(Guid feedId, byte[] media, string? fileName, string? mediaType)
        {
            return await UploadFeedMediaAsync(feedId, media, fileName ?? "media", mediaType).ConfigureAwait(false);
        }

        private async Task<string> UploadFeedMediaAsync(Guid feedId, byte[] media, string fileName, string? mediaType)
        {
            try
            {
                string connectionString = _configuration["BlobStorage:ConnectionString"] ?? _configuration["AzureWebJobsStorage"];
                string containerName = _configuration["BlobStorage:FeedMediaContainer"] ?? "feed-media";
                string extension = Path.GetExtension(fileName);
                string blobName = $"{feedId}{extension}";

                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None).ConfigureAwait(false);
                var blobClient = containerClient.GetBlobClient(blobName);

                using (var ms = new MemoryStream(media))
                {
                    await blobClient.UploadAsync(ms, new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders { ContentType = mediaType ?? "application/octet-stream" }
                    }).ConfigureAwait(false);
                }
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading feed media to blob for FeedId {FeedId}", feedId);
                throw;
            }
        }

        public async Task<IEnumerable<Feed>> SearchFeedsAsync(SearchFeedRequest request)
        {
            try
            {
                var query = _context.Feeds.AsQueryable();
                if (!string.IsNullOrEmpty(request.Title))
                    query = query.Where(f => f.Title.Contains(request.Title));
                if (!string.IsNullOrEmpty(request.Description))
                    query = query.Where(f => f.Description != null && f.Description.Contains(request.Description));
                if (request.UserId.HasValue)
                    query = query.Where(f => f.UserId == request.UserId);
                if (!string.IsNullOrEmpty(request.Hashtag))
                    query = query.Where(f => f.Description != null && f.Description.Contains("#" + request.Hashtag));
                query = query.Where(f => f.IsActive && !f.IsDeleted);
                query = query.OrderByDescending(f => f.CreatedAt);
                if (request.PageNumber > 0 && request.PageSize > 0)
                    query = query.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize);
                return await query.ToListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching feeds");
                throw;
            }
        }

        public async Task<IEnumerable<Feed>> FindFeedsByFieldsAsync(Dictionary<string, object> filters)
        {
            var parameter = Expression.Parameter(typeof(Feed), "e");
            Expression predicate = Expression.AndAlso(
                Expression.Equal(Expression.Property(parameter, nameof(Feed.IsActive)), Expression.Constant(true)),
                Expression.Equal(Expression.Property(parameter, nameof(Feed.IsDeleted)), Expression.Constant(false))
            );

            foreach (var filter in filters)
            {
                var property = typeof(Feed).GetProperty(filter.Key);
                if (property == null) continue;
                var left = Expression.Property(parameter, property);
                var right = Expression.Constant(ValueConverter.ConvertValueToType(filter.Value, property.PropertyType));
                var equals = Expression.Equal(left, right);
                predicate = Expression.AndAlso(predicate, equals);
            }

            var lambda = Expression.Lambda<Func<Feed, bool>>(predicate, parameter);
            return await _context.Feeds.Where(lambda).OrderByDescending(f => f.CreatedAt).ToListAsync();
        }

        // Likes
        public async Task<bool> AddLikeAsync(Guid feedId, Guid userId, string? ip = null)
        {
            try
            {
                var existing = await _context.FeedLikes.FirstOrDefaultAsync(l => l.FeedId == feedId && l.UserId == userId && l.IsActive && !l.IsDeleted);
                if (existing != null) return true; // idempotente
                var like = new FeedLike { Id = Guid.NewGuid(), FeedId = feedId, UserId = userId, CreatedAt = DateTime.UtcNow, Ip = ip, IsActive = true, IsDeleted = false };
                _context.FeedLikes.Add(like);
                var feed = await _context.Feeds.FirstOrDefaultAsync(f => f.Id == feedId);
                if (feed != null) feed.LikesCount += 1;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding like FeedId={FeedId} UserId={UserId}", feedId, userId);
                return false;
            }
        }

        public async Task<bool> RemoveLikeAsync(Guid feedId, Guid userId)
        {
            try
            {
                var existing = await _context.FeedLikes.FirstOrDefaultAsync(l => l.FeedId == feedId && l.UserId == userId && l.IsActive && !l.IsDeleted);
                if (existing == null) return true; // idempotente
                existing.IsActive = false;
                existing.IsDeleted = true;
                existing.DeletedAt = DateTime.UtcNow;
                var feed = await _context.Feeds.FirstOrDefaultAsync(f => f.Id == feedId);
                if (feed != null && feed.LikesCount > 0) feed.LikesCount -= 1;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing like FeedId={FeedId} UserId={UserId}", feedId, userId);
                return false;
            }
        }

        public async Task<int> GetLikesCountAsync(Guid feedId)
        {
            return await _context.FeedLikes.CountAsync(l => l.FeedId == feedId && l.IsActive && !l.IsDeleted);
        }

        // Comments
        public async Task<FeedComment> AddCommentAsync(FeedComment comment)
        {
            comment.Id = Guid.NewGuid();
            comment.CreatedAt = DateTime.UtcNow;
            comment.IsActive = true;
            comment.IsDeleted = false;
            _context.FeedComments.Add(comment);
            var feed = await _context.Feeds.FirstOrDefaultAsync(f => f.Id == comment.FeedId);
            if (feed != null) feed.CommentsCount += 1;
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var existing = await _context.FeedComments.FirstOrDefaultAsync(c => c.Id == commentId && c.UserId == userId && c.IsActive && !c.IsDeleted);
            if (existing == null) return false;
            existing.IsActive = false;
            existing.IsDeleted = true;
            existing.DeletedAt = DateTime.UtcNow;
            var feed = await _context.Feeds.FirstOrDefaultAsync(f => f.Id == existing.FeedId);
            if (feed != null && feed.CommentsCount > 0) feed.CommentsCount -= 1;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FeedComment>> GetCommentsAsync(Guid feedId, int page = 1, int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 50;
            return await _context.FeedComments
                .Where(c => c.FeedId == feedId && c.IsActive && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCommentsCountAsync(Guid feedId)
        {
            return await _context.FeedComments.CountAsync(c => c.FeedId == feedId && c.IsActive && !c.IsDeleted);
        }
    }
}

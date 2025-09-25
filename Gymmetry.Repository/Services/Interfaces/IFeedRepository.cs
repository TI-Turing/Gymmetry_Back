using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO.Feed.Request;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Cache;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IFeedRepository
    {
        Task<Feed> AddFeedAsync(Feed feed, byte[]? media = null, string? fileName = null, string? mediaType = null);
        Task<bool> UpdateFeedAsync(Feed feed, byte[]? media = null, string? fileName = null, string? mediaType = null);
        Task<bool> DeleteFeedAsync(Guid feedId);
        Task<Feed?> GetFeedByIdAsync(Guid feedId);
        Task<IEnumerable<Feed>> GetFeedsByUserAsync(Guid userId);
        Task<IEnumerable<Feed>> GetAllFeedsAsync();
        Task<string> UploadFeedMediaToBlobAsync(Guid feedId, byte[] media, string? fileName, string? mediaType);
        Task<IEnumerable<Feed>> SearchFeedsAsync(SearchFeedRequest request);
        Task<IEnumerable<Feed>> FindFeedsByFieldsAsync(Dictionary<string, object> filters);
        Task<Feed> CreateFeedWithMediaAsync(Feed feed, List<FeedMedia> mediaFiles);

        // Likes
        Task<bool> AddLikeAsync(Guid feedId, Guid userId, string? ip = null);
        Task<bool> RemoveLikeAsync(Guid feedId, Guid userId);
        Task<int> GetLikesCountAsync(Guid feedId);

        // Comments
        Task<FeedComment> AddCommentAsync(FeedComment comment);
        Task<bool> DeleteCommentAsync(Guid commentId, Guid userId);
        Task<IEnumerable<FeedComment>> GetCommentsAsync(Guid feedId, int page = 1, int pageSize = 50);
        Task<int> GetCommentsCountAsync(Guid feedId);
    }
}

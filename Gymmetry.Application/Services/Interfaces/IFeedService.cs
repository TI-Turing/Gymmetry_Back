using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Feed.Request;
using Gymmetry.Domain.DTO.Feed.Response;
using Gymmetry.Domain.Models;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IFeedService
    {
        // Core CRUD operations (existing)
        Task<ApplicationResponse<Feed>> CreateFeedAsync(Feed feed, byte[]? media = null, string? fileName = null, string? mediaType = null);
        Task<ApplicationResponse<bool>> UpdateFeedAsync(Feed feed, byte[]? media = null, string? fileName = null, string? mediaType = null, Guid? userId = null, string ip = "", string invocationId = "");
        Task<ApplicationResponse<bool>> DeleteFeedAsync(Guid feedId);
        Task<ApplicationResponse<Feed>> GetFeedByIdAsync(Guid feedId);
        Task<ApplicationResponse<IEnumerable<Feed>>> GetFeedsByUserAsync(Guid userId);
        Task<ApplicationResponse<IEnumerable<Feed>>> GetAllFeedsAsync();
        Task<ApplicationResponse<string>> UploadFeedMediaAsync(Guid feedId, byte[] media, string? fileName, string? contentType);
        Task<ApplicationResponse<IEnumerable<Feed>>> SearchFeedsAsync(string? title, string? description, Guid? userId, string? hashtag, int pageNumber = 0, int pageSize = 0);
        Task<ApplicationResponse<IEnumerable<Feed>>> FindFeedsByFieldsAsync(Dictionary<string, object> filters);
        Task<ApplicationResponse<PagedResult<Feed>>> GetGlobalFeedPagedAsync(int pageNumber, int pageSize, Guid? viewerUserId = null);
        Task<ApplicationResponse<PagedResult<Feed>>> GetUserFeedPagedAsync(Guid userId, int pageNumber, int pageSize, Guid? viewerUserId = null);
        Task<ApplicationResponse<IEnumerable<Feed>>> GetTrendingFeedAsync(int hoursWindow = 24, int take = 20);
        Task<ApplicationResponse<bool>> AutoPostProgressMilestoneAsync(Guid userId, string milestoneCode, string? extra = null);

        // Business logic methods for handling DTOs and request processing
        Task<ApplicationResponse<Feed>> CreateFeedFromRequestAsync(FeedCreateRequestDto request, Guid userId, string? clientIp = null);
        Task<ApplicationResponse<bool>> UpdateFeedFromRequestAsync(Guid feedId, FeedUpdateRequestDto request, Guid userId, string? clientIp = null, string? invocationId = null);
        Task<ApplicationResponse<Feed>> SearchFeedsFromRequestAsync(SearchFeedRequest request, Guid userId);

        // Multimedia feed creation
        Task<ApplicationResponse<FeedWithMediaResponse>> CreateFeedWithMediaAsync(CreateFeedWithMediaRequest request, Guid userId, string? clientIp = null);

        // Likes
        Task<ApplicationResponse<bool>> LikeAsync(Guid feedId, Guid userId, string? ip = null);
        Task<ApplicationResponse<bool>> UnlikeAsync(Guid feedId, Guid userId);
        Task<ApplicationResponse<int>> GetLikesCountAsync(Guid feedId);

        // Comments
        Task<ApplicationResponse<FeedComment>> AddCommentAsync(Guid feedId, Guid userId, string content, bool isAnonymous = false, string? ip = null);
        Task<ApplicationResponse<bool>> DeleteCommentAsync(Guid commentId, Guid userId);
        Task<ApplicationResponse<IEnumerable<FeedComment>>> GetCommentsAsync(Guid feedId, int page = 1, int pageSize = 50);
        Task<ApplicationResponse<int>> GetCommentsCountAsync(Guid feedId);
    }

    public class PagedResult<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    }
}

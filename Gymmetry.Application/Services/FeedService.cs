using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Application.Services
{
    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _feedRepository;
        private readonly ILogger<FeedService> _logger;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        // private readonly IRedisCacheService _redis; // Comentado hasta habilitar

        public FeedService(IFeedRepository feedRepository, ILogger<FeedService> logger, ILogChangeService logChangeService, ILogErrorService logErrorService /*, IRedisCacheService redis */)
        {
            _feedRepository = feedRepository;
            _logger = logger;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            // _redis = redis;
        }

        public async Task<ApplicationResponse<Feed>> CreateFeedAsync(Feed feed, byte[]? media = null, string? fileName = null, string? mediaType = null)
        {
            try
            {
                if (feed == null)
                    return ApplicationResponse<Feed>.ErrorResponse("Feed nulo", "BadRequest");
                feed.CreatedAt = DateTime.UtcNow;
                feed.IsActive = true;
                var created = await _feedRepository.AddFeedAsync(feed, media, fileName, mediaType);
                // await InvalidateGlobalCacheAsync();
                return ApplicationResponse<Feed>.SuccessResponse(created, "Feed creado correctamente.");
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<Feed>.ErrorResponse("Error técnico al crear el feed.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> UpdateFeedAsync(Feed feed, byte[]? media = null, string? fileName = null, string? mediaType = null, Guid? userId = null, string ip = "", string invocationId = "")
        {
            try
            {
                if (feed == null || feed.Id == Guid.Empty)
                    return ApplicationResponse<bool>.ErrorResponse("Datos inválidos para actualizar el feed.", "BadRequest");
                var before = await _feedRepository.GetFeedByIdAsync(feed.Id);
                var updated = await _feedRepository.UpdateFeedAsync(feed, media, fileName, mediaType);
                if (!updated)
                    return ApplicationResponse<bool>.ErrorResponse("No se pudo actualizar el feed (no encontrado o inactivo).", "NotFound");
                await _logChangeService.LogChangeAsync("Feed", before!, userId, ip, invocationId);
                // await InvalidateFeedCachesAsync(feed.Id, feed.UserId);
                return ApplicationResponse<bool>.SuccessResponse(true, "Feed actualizado correctamente.");
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al actualizar el feed.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteFeedAsync(Guid feedId)
        {
            try
            {
                var deleted = await _feedRepository.DeleteFeedAsync(feedId);
                if (!deleted)
                    return ApplicationResponse<bool>.ErrorResponse("No se pudo eliminar el feed (no encontrado o inactivo).", "NotFound");
                // await InvalidateFeedCachesAsync(feedId, null);
                return ApplicationResponse<bool>.SuccessResponse(true, "Feed eliminado correctamente.");
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al eliminar el feed.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<Feed>> GetFeedByIdAsync(Guid feedId)
        {
            try
            {
                var feed = await _feedRepository.GetFeedByIdAsync(feedId);
                if (feed == null)
                    return ApplicationResponse<Feed>.ErrorResponse("Feed no encontrado.", "NotFound");
                return ApplicationResponse<Feed>.SuccessResponse(feed);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<Feed>.ErrorResponse("Error técnico al obtener el feed.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<Feed>>> GetFeedsByUserAsync(Guid userId)
        {
            try
            {
                var feeds = await _feedRepository.GetFeedsByUserAsync(userId);
                return ApplicationResponse<IEnumerable<Feed>>.SuccessResponse(feeds);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<Feed>>.ErrorResponse("Error técnico al obtener los feeds del usuario.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<Feed>>> GetAllFeedsAsync()
        {
            try
            {
                var feeds = await _feedRepository.GetAllFeedsAsync();
                return ApplicationResponse<IEnumerable<Feed>>.SuccessResponse(feeds);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<Feed>>.ErrorResponse("Error técnico al obtener los feeds.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<string>> UploadFeedMediaAsync(Guid feedId, byte[] media, string? fileName, string? contentType)
        {
            try
            {
                var url = await _feedRepository.UploadFeedMediaToBlobAsync(feedId, media, fileName, contentType);
                return new ApplicationResponse<string>
                {
                    Success = true,
                    Data = url,
                    Message = "Media subida correctamente."
                };
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return new ApplicationResponse<string>
                {
                    Success = false,
                    Message = "Error técnico al subir el archivo multimedia.",
                    ErrorCode = "TechnicalError"
                };
            }
        }

        public async Task<ApplicationResponse<IEnumerable<Feed>>> SearchFeedsAsync(string? title, string? description, Guid? userId, string? hashtag, int pageNumber = 0, int pageSize = 0)
        {
            try
            {
                var req = new Gymmetry.Domain.DTO.Feed.Request.SearchFeedRequest
                {
                    Title = title,
                    Description = description,
                    UserId = userId,
                    Hashtag = hashtag,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
                var feeds = await _feedRepository.SearchFeedsAsync(req);
                return ApplicationResponse<IEnumerable<Feed>>.SuccessResponse(feeds);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<Feed>>.ErrorResponse("Error técnico al buscar feeds.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<Feed>>> FindFeedsByFieldsAsync(Dictionary<string, object> filters)
        {
            var feeds = await _feedRepository.FindFeedsByFieldsAsync(filters);
            return ApplicationResponse<IEnumerable<Feed>>.SuccessResponse(feeds);
        }

        public async Task<ApplicationResponse<PagedResult<Feed>>> GetGlobalFeedPagedAsync(int pageNumber, int pageSize, Guid? viewerUserId = null)
        {
            try
            {
                if (pageNumber < 1 || pageSize <= 0) return ApplicationResponse<PagedResult<Feed>>.ErrorResponse("Parámetros de paginación inválidos", "BadRequest");
                var all = await _feedRepository.GetAllFeedsAsync();
                var total = all.Count();
                var items = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                var result = new PagedResult<Feed>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                    Items = items
                };
                return ApplicationResponse<PagedResult<Feed>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<PagedResult<Feed>>.ErrorResponse("Error técnico al obtener feed paginado.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<PagedResult<Feed>>> GetUserFeedPagedAsync(Guid userId, int pageNumber, int pageSize, Guid? viewerUserId = null)
        {
            try
            {
                if (pageNumber < 1 || pageSize <= 0) return ApplicationResponse<PagedResult<Feed>>.ErrorResponse("Parámetros de paginación inválidos", "BadRequest");
                var all = await _feedRepository.GetFeedsByUserAsync(userId);
                var total = all.Count();
                var items = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                var result = new PagedResult<Feed>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                    Items = items
                };
                return ApplicationResponse<PagedResult<Feed>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<PagedResult<Feed>>.ErrorResponse("Error técnico al obtener feed paginado de usuario.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<Feed>>> GetTrendingFeedAsync(int hoursWindow = 24, int take = 20)
        {
            try
            {
                var since = DateTime.UtcNow.AddHours(-Math.Abs(hoursWindow));
                var all = await _feedRepository.GetAllFeedsAsync();
                var window = all.Where(f => f.CreatedAt >= since);
                // Score simple: LikesCount*3 + CommentsCount*2 + recency boost
                var ranked = window.Select(f => new { Feed = f, Score = f.LikesCount * 3 + f.CommentsCount * 2 + (int)(24 - (DateTime.UtcNow - f.CreatedAt).TotalHours) })
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.Feed.CreatedAt)
                    .Take(take)
                    .Select(x => x.Feed)
                    .ToList();
                return ApplicationResponse<IEnumerable<Feed>>.SuccessResponse(ranked);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<Feed>>.ErrorResponse("Error técnico al obtener trending feed.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> AutoPostProgressMilestoneAsync(Guid userId, string milestoneCode, string? extra = null)
        {
            try
            {
                var templates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["STREAK_7"] = "?? ¡He logrado una racha de 7 días entrenando!",
                    ["STREAK_30"] = "?? 30 días de constancia alcanzados!",
                    ["WEIGHT_LOSS_5"] = "?? He reducido 5 kg en mi progreso!",
                    ["FIRST_ASSESSMENT"] = "?? Realicé mi primera evaluación física.",
                    ["ROUTINE_COMPLETED"] = "? Completé mi rutina asignada!"
                };
                if (!templates.TryGetValue(milestoneCode, out var msg)) msg = "Progreso alcanzado";
                if (!string.IsNullOrWhiteSpace(extra)) msg += " - " + extra;
                var feed = new Feed
                {
                    UserId = userId,
                    Title = milestoneCode,
                    Description = msg,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    IsAnonymous = false
                };
                await _feedRepository.AddFeedAsync(feed);
                return ApplicationResponse<bool>.SuccessResponse(true, "Milestone publicado.");
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al autopublicar milestone.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> LikeAsync(Guid feedId, Guid userId, string? ip = null)
        {
            try
            {
                var ok = await _feedRepository.AddLikeAsync(feedId, userId, ip);
                return ok ? ApplicationResponse<bool>.SuccessResponse(true, "Like aplicado.") : ApplicationResponse<bool>.ErrorResponse("No se pudo aplicar like.");
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al registrar like.");
            }
        }

        public async Task<ApplicationResponse<bool>> UnlikeAsync(Guid feedId, Guid userId)
        {
            try
            {
                var ok = await _feedRepository.RemoveLikeAsync(feedId, userId);
                return ok ? ApplicationResponse<bool>.SuccessResponse(true, "Like removido.") : ApplicationResponse<bool>.ErrorResponse("No se pudo remover like.");
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al remover like.");
            }
        }

        public async Task<ApplicationResponse<int>> GetLikesCountAsync(Guid feedId)
        {
            try
            {
                var c = await _feedRepository.GetLikesCountAsync(feedId);
                return ApplicationResponse<int>.SuccessResponse(c);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<int>.ErrorResponse("Error técnico al obtener likes.");
            }
        }

        public async Task<ApplicationResponse<FeedComment>> AddCommentAsync(Guid feedId, Guid userId, string content, bool isAnonymous = false, string? ip = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content)) return ApplicationResponse<FeedComment>.ErrorResponse("Contenido vacío.");
                var comment = new FeedComment
                {
                    FeedId = feedId,
                    UserId = userId,
                    Content = content,
                    IsAnonymous = isAnonymous,
                    Ip = ip
                };
                var created = await _feedRepository.AddCommentAsync(comment);
                return ApplicationResponse<FeedComment>.SuccessResponse(created, "Comentario agregado.");
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<FeedComment>.ErrorResponse("Error técnico al agregar comentario.");
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteCommentAsync(Guid commentId, Guid userId)
        {
            try
            {
                var ok = await _feedRepository.DeleteCommentAsync(commentId, userId);
                return ok ? ApplicationResponse<bool>.SuccessResponse(true, "Comentario eliminado.") : ApplicationResponse<bool>.ErrorResponse("No se pudo eliminar comentario.");
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al eliminar comentario.");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<FeedComment>>> GetCommentsAsync(Guid feedId, int page = 1, int pageSize = 50)
        {
            try
            {
                var list = await _feedRepository.GetCommentsAsync(feedId, page, pageSize);
                return ApplicationResponse<IEnumerable<FeedComment>>.SuccessResponse(list);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<FeedComment>>.ErrorResponse("Error técnico al obtener comentarios.");
            }
        }

        public async Task<ApplicationResponse<int>> GetCommentsCountAsync(Guid feedId)
        {
            try
            {
                var c = await _feedRepository.GetCommentsCountAsync(feedId);
                return ApplicationResponse<int>.SuccessResponse(c);
            }
            catch (Exception ex)
            {
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<int>.ErrorResponse("Error técnico al obtener total de comentarios.");
            }
        }

        // private async Task InvalidateFeedCachesAsync(Guid feedId, Guid? userId)
        // {
        //     try
        //     {
        //         await _redis.RemoveAsync($"feed:{feedId}");
        //         if (userId.HasValue) await _redis.RemoveAsync($"user:feeds:{userId}");
        //         await _redis.RemoveAsync("global:feed:paged:1:20");
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogWarning(ex, "Fallo al invalidar cache feed");
        //     }
        // }

        // private async Task InvalidateGlobalCacheAsync()
        // {
        //     try { await _redis.RemoveAsync("global:feed:paged:1:20"); } catch { }
        // }
    }
}

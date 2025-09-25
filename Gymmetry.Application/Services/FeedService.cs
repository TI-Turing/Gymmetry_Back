using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.Feed.Request;
using Gymmetry.Domain.DTO.Feed.Response;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Gymmetry.Application.Services
{
    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _feedRepository;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IMediaProcessingService _mediaProcessingService;
        private readonly ILogger<FeedService> _logger;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        // private readonly IRedisCacheService _redis; // Comentado hasta habilitar

        public FeedService(
            IFeedRepository feedRepository, 
            IBlobStorageService blobStorageService,
            IMediaProcessingService mediaProcessingService,
            ILogger<FeedService> logger, 
            ILogChangeService logChangeService, 
            ILogErrorService logErrorService /*, IRedisCacheService redis */)
        {
            _feedRepository = feedRepository;
            _blobStorageService = blobStorageService;
            _mediaProcessingService = mediaProcessingService;
            _logger = logger;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            // _redis = redis;
        }

        #region NEW: Multimedia Feed Creation

        public async Task<ApplicationResponse<FeedWithMediaResponse>> CreateFeedWithMediaAsync(CreateFeedWithMediaRequest request, Guid userId, string? clientIp = null)
        {
            try
            {
                _logger.LogInformation("Creating multimedia feed for user {UserId} with {FileCount} files", userId, request.Files?.Length ?? 0);

                // 1. Validar request básico
                var validationResult = await ValidateCreateFeedWithMediaRequestAsync(request);
                if (!validationResult.Success)
                {
                    return ApplicationResponse<FeedWithMediaResponse>.ErrorResponse(validationResult.Message!, validationResult.ErrorCode);
                }

                // 2. Procesar archivos multimedia
                var processedFiles = new List<ProcessedMediaFile>();
                var uploadedFiles = new List<(ProcessedMediaFile file, string blobName, string publicUrl)>();

                try
                {
                    // Procesar cada archivo
                    foreach (var file in request.Files!)
                    {
                        var validation = await _mediaProcessingService.ValidateMediaFileAsync(file);
                        if (!validation.IsValid)
                        {
                            throw new InvalidOperationException($"Archivo {file.FileName}: {validation.ErrorMessage}");
                        }

                        ProcessedMediaFile processedFile;
                        if (validation.MediaType == "image")
                        {
                            var options = new MediaProcessingOptions
                            {
                                MaxWidth = 1920,
                                MaxHeight = 1080,
                                JpegQuality = 85,
                                MaxSizeBytes = 500 * 1024 // 500KB para imágenes
                            };
                            processedFile = await _mediaProcessingService.ProcessImageAsync(file, options);
                        }
                        else // video
                        {
                            var options = new MediaProcessingOptions
                            {
                                MaxSizeBytes = 15 * 1024 * 1024, // 15MB para videos
                                MaxDurationSeconds = 180 // 3 minutos
                            };
                            processedFile = await _mediaProcessingService.ProcessVideoAsync(file, options);
                        }

                        processedFiles.Add(processedFile);
                    }

                    // 3. Subir archivos a Blob Storage
                    foreach (var processedFile in processedFiles)
                    {
                        var blobName = GenerateUniqueBlobName(processedFile.OriginalFileName, processedFile.FileExtension);
                        var publicUrl = await _blobStorageService.UploadAsync("feed-media", blobName, processedFile.Data, processedFile.ContentType);
                        uploadedFiles.Add((processedFile, blobName, publicUrl));

                        _logger.LogInformation("Uploaded media file {FileName} as {BlobName}, size: {Size} bytes", 
                            processedFile.OriginalFileName, blobName, processedFile.SizeBytes);
                    }

                    // 4. Crear Feed en base de datos (con transacción)
                    var feed = new Feed
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Title = null, // Posts de multimedia no usan título
                        Description = request.Description.Trim(),
                        MediaType = DetermineMediaType(processedFiles),
                        IsAnonymous = request.IsAnonymous,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false,
                        Ip = clientIp
                    };

                    // Crear registros FeedMedia
                    var feedMediaRecords = uploadedFiles.Select(item => new FeedMedia
                    {
                        Id = Guid.NewGuid(),
                        FeedId = feed.Id,
                        MediaUrl = item.publicUrl,
                        MediaType = item.file.ContentType,
                        FileName = item.file.OriginalFileName,
                        FileSizeBytes = item.file.SizeBytes,
                        BlobName = item.blobName,
                        DurationSeconds = item.file.DurationSeconds,
                        Width = item.file.Width,
                        Height = item.file.Height,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    }).ToList();

                    // Guardar en repositorio (implementar transacción)
                    var createdFeed = await _feedRepository.CreateFeedWithMediaAsync(feed, feedMediaRecords);

                    // 5. Construir respuesta
                    var response = new FeedWithMediaResponse
                    {
                        Id = createdFeed.Id,
                        UserId = createdFeed.UserId,
                        Description = createdFeed.Description ?? string.Empty,
                        IsAnonymous = createdFeed.IsAnonymous,
                        Hashtags = request.Hashtags,
                        CreatedAt = createdFeed.CreatedAt,
                        LikesCount = 0,
                        CommentsCount = 0,
                        MediaFiles = feedMediaRecords.Select(m => new MediaFileInfo
                        {
                            Url = m.MediaUrl,
                            MediaType = m.MediaType,
                            FileName = m.FileName,
                            SizeBytes = m.FileSizeBytes,
                            DurationSeconds = m.DurationSeconds,
                            Width = m.Width,
                            Height = m.Height
                        }).ToList()
                    };

                    _logger.LogInformation("Successfully created multimedia feed {FeedId} with {MediaCount} media files", 
                        feed.Id, feedMediaRecords.Count);

                    return ApplicationResponse<FeedWithMediaResponse>.SuccessResponse(response, "Post con multimedia creado exitosamente.");
                }
                catch (Exception ex)
                {
                    // Rollback: eliminar archivos subidos en caso de error
                    await CleanupUploadedFilesAsync(uploadedFiles.Select(u => u.blobName).ToList());
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating multimedia feed for user {UserId}", userId);
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<FeedWithMediaResponse>.ErrorResponse(
                    "Error técnico al crear el post con multimedia.", "TechnicalError");
            }
        }

        private async Task<ApplicationResponse<bool>> ValidateCreateFeedWithMediaRequestAsync(CreateFeedWithMediaRequest request)
        {
            if (request == null)
                return ApplicationResponse<bool>.ErrorResponse("Los datos del post son nulos.", "BadRequest");

            if (string.IsNullOrWhiteSpace(request.Description))
                return ApplicationResponse<bool>.ErrorResponse("La descripción es requerida para posts con multimedia.", "BadRequest");

            if (request.Description.Length > 2000)
                return ApplicationResponse<bool>.ErrorResponse("La descripción no puede exceder 2000 caracteres.", "BadRequest");

            if (request.Files == null || !request.Files.Any())
                return ApplicationResponse<bool>.ErrorResponse("Al menos un archivo multimedia es requerido.", "BadRequest");

            if (request.Files.Length > 5)
                return ApplicationResponse<bool>.ErrorResponse("Máximo 5 archivos por post.", "BadRequest");

            // Validar peso total
            var totalSizeBytes = request.Files.Sum(f => f.Length);
            if (totalSizeBytes > 25 * 1024 * 1024) // 25MB total
                return ApplicationResponse<bool>.ErrorResponse("El peso total de archivos no puede superar 25MB.", "BadRequest");

            return ApplicationResponse<bool>.SuccessResponse(true);
        }

        private string DetermineMediaType(List<ProcessedMediaFile> files)
        {
            var hasImages = files.Any(f => f.ContentType.StartsWith("image/"));
            var hasVideos = files.Any(f => f.ContentType.StartsWith("video/"));

            if (hasImages && hasVideos) return "mixed";
            if (hasImages) return "image";
            if (hasVideos) return "video";
            return "unknown";
        }

        private string GenerateUniqueBlobName(string originalFileName, string fileExtension)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var sanitizedFileName = Path.GetFileNameWithoutExtension(originalFileName)
                .Replace(" ", "_")
                .Replace("(", "")
                .Replace(")", "");
            
            return $"feeds/{timestamp}_{uniqueId}_{sanitizedFileName}{fileExtension}";
        }

        private async Task CleanupUploadedFilesAsync(List<string> blobNames)
        {
            foreach (var blobName in blobNames)
            {
                try
                {
                    await _blobStorageService.DeleteIfExistsAsync("feed-media", blobName);
                    _logger.LogInformation("Cleaned up blob {BlobName}", blobName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cleanup blob {BlobName}: {Error}", blobName, ex.Message);
                }
            }
        }

        #endregion

        #region Business Logic Methods (for Azure Functions)

        public async Task<ApplicationResponse<Feed>> CreateFeedFromRequestAsync(FeedCreateRequestDto request, Guid userId, string? clientIp = null)
        {
            try
            {
                _logger.LogInformation("Creating feed from request for user {UserId}", userId);

                // Validate request
                if (request == null)
                    return ApplicationResponse<Feed>.ErrorResponse("Los datos del feed son nulos o inválidos.", "BadRequest");

                if (string.IsNullOrWhiteSpace(request.Title))
                    return ApplicationResponse<Feed>.ErrorResponse("El título del feed es requerido.", "BadRequest");

                if (request.Title.Length > 200)
                    return ApplicationResponse<Feed>.ErrorResponse("El título no puede exceder 200 caracteres.", "BadRequest");

                if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > 2000)
                    return ApplicationResponse<Feed>.ErrorResponse("La descripción no puede exceder 2000 caracteres.", "BadRequest");

                // Get media bytes from base64 if present
                byte[]? mediaBytes = request.GetMediaBytes();
                
                if (mediaBytes != null)
                {
                    _logger.LogInformation("Media detected: {MediaSize} bytes, type: {MediaType}", 
                        mediaBytes.Length, request.MediaType ?? "unknown");
                        
                    // Validate media size (max 50MB)
                    const int maxMediaSize = 50 * 1024 * 1024; // 50MB
                    if (mediaBytes.Length > maxMediaSize)
                    {
                        return ApplicationResponse<Feed>.ErrorResponse(
                            $"El archivo de media es demasiado grande. Máximo permitido: {maxMediaSize / (1024 * 1024)}MB.", 
                            "BadRequest");
                    }
                }

                // Create Feed entity
                var entity = new Feed
                {
                    UserId = userId,
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim(),
                    Ip = clientIp
                };

                // Call core service method
                var result = await CreateFeedAsync(entity, mediaBytes, request.FileName, request.MediaType);
                
                _logger.LogInformation("Feed creation result: Success={Success}, Message={Message}", 
                    result.Success, result.Message);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating feed from request for user {UserId}", userId);
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<Feed>.ErrorResponse("Ocurrió un error interno al procesar la solicitud.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> UpdateFeedFromRequestAsync(Guid feedId, FeedUpdateRequestDto request, Guid userId, string? clientIp = null, string? invocationId = null)
        {
            try
            {
                _logger.LogInformation("Updating feed {FeedId} from request for user {UserId}", feedId, userId);

                // Validate request
                if (request == null)
                    return ApplicationResponse<bool>.ErrorResponse("Los datos de actualización son nulos.", "BadRequest");

                // Get current feed to validate ownership
                var currentFeed = await _feedRepository.GetFeedByIdAsync(feedId);
                if (currentFeed == null)
                    return ApplicationResponse<bool>.ErrorResponse("Feed no encontrado.", "NotFound");

                if (currentFeed.UserId != userId)
                    return ApplicationResponse<bool>.ErrorResponse("No tiene permisos para actualizar este feed.", "Forbidden");

                // Validate fields if provided
                if (!string.IsNullOrEmpty(request.Title) && request.Title.Length > 200)
                    return ApplicationResponse<bool>.ErrorResponse("El título no puede exceder 200 caracteres.", "BadRequest");

                if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > 2000)
                    return ApplicationResponse<bool>.ErrorResponse("La descripción no puede exceder 2000 caracteres.", "BadRequest");

                // Create updated entity
                var entity = new Feed
                {
                    Id = feedId,
                    Title = !string.IsNullOrEmpty(request.Title) ? request.Title.Trim() : currentFeed.Title,
                    Description = request.Description?.Trim() ?? currentFeed.Description,
                    UserId = currentFeed.UserId, // Preserve original user
                    CreatedAt = currentFeed.CreatedAt, // Preserve creation date
                    UpdatedAt = DateTime.UtcNow,
                    Ip = clientIp
                };

                // Get media bytes if provided
                byte[]? mediaBytes = request.GetMediaBytes();
                if (mediaBytes != null)
                {
                    const int maxMediaSize = 50 * 1024 * 1024; // 50MB
                    if (mediaBytes.Length > maxMediaSize)
                    {
                        return ApplicationResponse<bool>.ErrorResponse(
                            $"El archivo de media es demasiado grande. Máximo permitido: {maxMediaSize / (1024 * 1024)}MB.", 
                            "BadRequest");
                    }
                }

                // Call core service method
                var result = await UpdateFeedAsync(entity, mediaBytes, request.FileName, request.MediaType, userId, clientIp, invocationId);
                
                _logger.LogInformation("Feed update result: Success={Success}, Message={Message}", 
                    result.Success, result.Message);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating feed {FeedId} for user {UserId}", feedId, userId);
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Ocurrió un error interno al actualizar el feed.", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<Feed>> SearchFeedsFromRequestAsync(SearchFeedRequest request, Guid userId)
        {
            try
            {
                _logger.LogInformation("Searching feeds from request for user {UserId}", userId);

                // Validate request
                if (request == null)
                    return ApplicationResponse<Feed>.ErrorResponse("Datos de búsqueda inválidos.", "BadRequest");

                // Set default pagination if not provided
                var pageNumber = request.PageNumber > 0 ? request.PageNumber : 1;
                var pageSize = request.PageSize > 0 ? request.PageSize : 20;

                // Validate pagination limits
                if (pageSize > 100)
                    return ApplicationResponse<Feed>.ErrorResponse("El tamaño de página no puede exceder 100.", "BadRequest");

                // Call core search method
                var result = await SearchFeedsAsync(request.Title, request.Description, request.UserId, request.Hashtag, pageNumber, pageSize);
                
                _logger.LogInformation("Feed search completed for user {UserId}: {Count} results", userId, result.Data?.Count() ?? 0);

                // Convert to single feed result (for compatibility, return first result)
                if (result.Success && result.Data?.Any() == true)
                {
                    return ApplicationResponse<Feed>.SuccessResponse(result.Data.First(), "Búsqueda completada exitosamente.");
                }

                return ApplicationResponse<Feed>.ErrorResponse("No se encontraron feeds que coincidan con los criterios de búsqueda.", "NotFound");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching feeds for user {UserId}", userId);
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<Feed>.ErrorResponse("Ocurrió un error interno al buscar feeds.", "TechnicalError");
            }
        }

        #endregion

        #region Core CRUD Operations

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
                var req = new SearchFeedRequest
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

        #endregion

        #region Likes

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

        #endregion

        #region Comments

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

        #endregion

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

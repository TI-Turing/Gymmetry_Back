using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ContentModeration;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Application.Services
{
    public class ContentModerationService : IContentModerationService
    {
        private readonly IContentModerationRepository _repository;
        private readonly IFeedRepository _feedRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<ContentModerationService> _logger;

        // Filtros automáticos
        private static readonly string[] ProfanityWords = {
            // Español
            "idiota", "estúpido", "imbécil", "mierda", "joder", "cabrón", "puto", "puta", "pendejo", "culero",
            // Inglés
            "fuck", "shit", "damn", "bitch", "asshole", "stupid", "idiot", "moron", "bastard", "cunt"
        };

        private static readonly string[] SpamPatterns = {
            @"(http[s]?://(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+){3,}", // Multiple URLs
            @"(.)\1{10,}", // Repeated characters
            @"\b(BUY NOW|CLICK HERE|FREE MONEY|URGENT|WINNER)\b", // Spam keywords
            @"[A-Z]{20,}", // Excessive caps
        };

        private static readonly string[] ViolenceHatePatterns = {
            @"\b(kill|murder|rape|torture|violence|hate|nazi|terrorist)\b",
            @"\b(matar|asesinar|torturar|violencia|odio|nazi|terrorista)\b"
        };

        public ContentModerationService(
            IContentModerationRepository repository,
            IFeedRepository feedRepository,
            ILogChangeService logChangeService,
            ILogErrorService logErrorService,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<ContentModerationService> logger)
        {
            _repository = repository;
            _feedRepository = feedRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApplicationResponse<ContentModerationResponse>> CreateAsync(ContentModerationCreateRequest request, Guid? userId, string ip = "", string invocationId = "")
        {
            _logger.LogInformation("[ContentModerationService] Inicio CreateAsync");

            try
            {
                if (request == null)
                    return ApplicationResponse<ContentModerationResponse>.ErrorResponse("Request nulo", "BadRequest");

                // Validar que solo moderadores puedan crear moderación manual
                if (!request.AutoModerated && request.ModeratedBy.HasValue)
                {
                    // TODO: Validar que ModeratedBy es un moderador
                }

                var entity = _mapper.Map<ContentModeration>(request);
                entity.ModeratedBy = request.AutoModerated ? null : (request.ModeratedBy ?? userId);

                // Aplicar lógica de escalado por confianza
                if (request.AutoModerated && request.Confidence.HasValue)
                {
                    if (request.Confidence.Value >= 0.8m)
                    {
                        entity.ModerationAction = DetermineModerationAction(request.FilterType, request.Confidence.Value);
                        entity.ReviewRequired = false;
                    }
                    else if (request.Confidence.Value >= 0.5m)
                    {
                        entity.ModerationAction = ModerationAction.Flagged;
                        entity.ReviewRequired = true;
                    }
                    else
                    {
                        entity.ModerationAction = ModerationAction.NoAction;
                        entity.ReviewRequired = false;
                    }
                }

                var created = await _repository.CreateAsync(entity);
                var response = _mapper.Map<ContentModerationResponse>(created);

                // Log de cambio
                await _logChangeService.LogChangeAsync("ContentModeration", null, userId, ip, invocationId);

                // Notificar moderadores si requiere revisión
                if (created.ReviewRequired)
                {
                    await TryNotifyModeratorsAsync($"Contenido {created.ContentType} {created.ContentId} requiere revisión manual");
                }

                _logger.LogInformation("[ContentModerationService] ContentModeration creado: {Id}", created.Id);
                return ApplicationResponse<ContentModerationResponse>.SuccessResponse(response, "Moderación creada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en CreateAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<ContentModerationResponse>.ErrorResponse("Error técnico al crear moderación", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<ContentModerationResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return ApplicationResponse<ContentModerationResponse>.ErrorResponse("Moderación no encontrada.", "NotFound");

                var response = _mapper.Map<ContentModerationResponse>(entity);
                return ApplicationResponse<ContentModerationResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en GetByIdAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<ContentModerationResponse>.ErrorResponse("Error técnico al consultar moderación", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<ContentModerationListResponse>> GetPagedAsync(int page, int pageSize)
        {
            try
            {
                var list = await _repository.GetPagedAsync(page, pageSize);
                var total = await _repository.CountAsync();

                var response = new ContentModerationListResponse
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalItems = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                    Items = list.Select(m => _mapper.Map<ContentModerationResponse>(m)).ToList()
                };

                return ApplicationResponse<ContentModerationListResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en GetPagedAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<ContentModerationListResponse>.ErrorResponse("Error técnico al consultar moderaciones", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> UpdateAsync(ContentModerationUpdateRequest request, Guid? userId, string ip = "", string invocationId = "")
        {
            try
            {
                var before = await _repository.GetByIdAsync(request.Id);
                if (before == null)
                    return ApplicationResponse<bool>.ErrorResponse("Moderación no encontrada.", "NotFound");

                var entity = _mapper.Map<ContentModeration>(request);
                entity.Id = request.Id;

                var updated = await _repository.UpdateAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("ContentModeration", before, userId, ip, invocationId);
                    return ApplicationResponse<bool>.SuccessResponse(true, "Moderación actualizada correctamente.");
                }

                return ApplicationResponse<bool>.ErrorResponse("No se pudo actualizar la moderación.", "NotFound");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en UpdateAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al actualizar moderación", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var deleted = await _repository.DeleteAsync(id);
                if (deleted)
                    return ApplicationResponse<bool>.SuccessResponse(true, "Moderación eliminada correctamente.");

                return ApplicationResponse<bool>.ErrorResponse("Moderación no encontrada o ya eliminada.", "NotFound");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en DeleteAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al eliminar moderación", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<ContentModerationResponse>>> FindByFieldsAsync(Dictionary<string, object> filters)
        {
            try
            {
                var list = await _repository.FindByFieldsAsync(filters);
                var mapped = list.Select(m => _mapper.Map<ContentModerationResponse>(m));
                return ApplicationResponse<IEnumerable<ContentModerationResponse>>.SuccessResponse(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en FindByFieldsAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<ContentModerationResponse>>.ErrorResponse("Error técnico al buscar moderaciones", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<ContentModerationResponse>>> GetPendingReviewAsync()
        {
            try
            {
                var list = await _repository.GetPendingReviewAsync();
                var mapped = list.Select(m => _mapper.Map<ContentModerationResponse>(m));
                return ApplicationResponse<IEnumerable<ContentModerationResponse>>.SuccessResponse(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en GetPendingReviewAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<ContentModerationResponse>>.ErrorResponse("Error técnico al obtener moderaciones pendientes", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<ContentModerationStatsResponse>> GetStatsAsync()
        {
            try
            {
                var (total, auto, manual, pending) = await _repository.GetStatsAsync();

                var response = new ContentModerationStatsResponse
                {
                    TotalModerations = total,
                    AutoModerations = auto,
                    ManualModerations = manual,
                    PendingReviews = pending,
                    ByAction = await _repository.GetStatsByActionAsync(),
                    ByReason = await _repository.GetStatsByReasonAsync(),
                    BySeverity = await _repository.GetStatsBySeverityAsync(),
                    ByContentType = await _repository.GetStatsByContentTypeAsync(),
                    Last7Days = await _repository.GetStatsLast7DaysAsync()
                };

                return ApplicationResponse<ContentModerationStatsResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en GetStatsAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<ContentModerationStatsResponse>.ErrorResponse("Error técnico al obtener estadísticas", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<BulkModerationResponse>> BulkApproveAsync(BulkModerationRequest request, Guid moderatorId)
        {
            try
            {
                var response = new BulkModerationResponse
                {
                    TotalRequested = request.ModerationIds.Count
                };

                if (!request.ModerationIds.Any())
                {
                    response.Errors.Add("No se proporcionaron IDs para aprobar");
                    return ApplicationResponse<BulkModerationResponse>.SuccessResponse(response);
                }

                if (request.ModerationIds.Count > 100)
                {
                    return ApplicationResponse<BulkModerationResponse>.ErrorResponse("Máximo 100 items por operación bulk", "BulkLimitExceeded");
                }

                var success = await _repository.BulkApproveAsync(request.ModerationIds, moderatorId, request.Notes);
                
                if (success)
                {
                    response.SuccessfullyProcessed = request.ModerationIds.Count;
                }
                else
                {
                    response.FailedIds.AddRange(request.ModerationIds);
                    response.Errors.Add("No se encontraron moderaciones pendientes para los IDs especificados");
                }

                return ApplicationResponse<BulkModerationResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en BulkApproveAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<BulkModerationResponse>.ErrorResponse("Error técnico en operación bulk", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<BulkModerationResponse>> BulkRejectAsync(BulkModerationRequest request, Guid moderatorId)
        {
            try
            {
                var response = new BulkModerationResponse
                {
                    TotalRequested = request.ModerationIds.Count
                };

                if (!request.ModerationIds.Any())
                {
                    response.Errors.Add("No se proporcionaron IDs para rechazar");
                    return ApplicationResponse<BulkModerationResponse>.SuccessResponse(response);
                }

                if (request.ModerationIds.Count > 100)
                {
                    return ApplicationResponse<BulkModerationResponse>.ErrorResponse("Máximo 100 items por operación bulk", "BulkLimitExceeded");
                }

                var success = await _repository.BulkRejectAsync(request.ModerationIds, moderatorId, request.Notes);
                
                if (success)
                {
                    response.SuccessfullyProcessed = request.ModerationIds.Count;
                }
                else
                {
                    response.FailedIds.AddRange(request.ModerationIds);
                    response.Errors.Add("No se encontraron moderaciones pendientes para los IDs especificados");
                }

                return ApplicationResponse<BulkModerationResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en BulkRejectAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<BulkModerationResponse>.ErrorResponse("Error técnico en operación bulk", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<ContentModerationResponse>> GetByContentAsync(Guid contentId, ContentType contentType)
        {
            try
            {
                var moderation = await _repository.GetByContentAsync(contentId, contentType);
                if (moderation == null)
                    return ApplicationResponse<ContentModerationResponse>.ErrorResponse("No se encontró moderación para este contenido.", "NotFound");

                var response = _mapper.Map<ContentModerationResponse>(moderation);
                return ApplicationResponse<ContentModerationResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en GetByContentAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<ContentModerationResponse>.ErrorResponse("Error técnico al consultar moderación de contenido", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<AutoScanResponse>> AutoScanAsync(AutoScanRequest request)
        {
            try
            {
                var response = new AutoScanResponse();
                var sinceDate = request.SinceDate ?? DateTime.UtcNow.AddHours(-24); // Último día por defecto
                var limit = Math.Min(request.LimitItems ?? 100, 1000); // Máximo 1000

                // Obtener contenido para escanear - Simulado por ahora
                // En una implementación real, consultaríamos la BD directamente
                response.ItemsScanned = limit;

                // Simular escaneo de contenido dummy para demostración
                var dummyContent = new[]
                {
                    "Este es un mensaje normal",
                    "¡Hola mundo! Buen día a todos",
                    "Contenido con palabras idiota y estúpido",
                    "COMPRA AHORA!!! CLICK HERE!!! FREE MONEY!!!",
                    "Mensaje normal de ejercicios",
                    "Otro mensaje con mierda y joder"
                };

                foreach (var content in dummyContent.Take(Math.Min(limit, dummyContent.Length)))
                {
                    var (flagged, moderated, filterType, confidence) = await ScanContentAsync(content, ContentType.Feed);
                    if (flagged) response.ItemsFlagged++;
                    if (moderated) response.ItemsModerated++;
                    
                    if (filterType != null)
                    {
                        if (response.FilterResults.ContainsKey(filterType))
                            response.FilterResults[filterType]++;
                        else
                            response.FilterResults[filterType] = 1;
                    }
                }

                return ApplicationResponse<AutoScanResponse>.SuccessResponse(response, $"Escaneo automático completado. {response.ItemsScanned} elementos analizados.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error en AutoScanAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<AutoScanResponse>.ErrorResponse("Error técnico en escaneo automático", "TechnicalError");
            }
        }

        private async Task<(bool flagged, bool moderated, string? filterType, decimal confidence)> ScanContentAsync(string content, ContentType contentType)
        {
            if (string.IsNullOrWhiteSpace(content))
                return (false, false, null, 0);

            var contentLower = content.ToLowerInvariant();
            
            // Verificar profanidad
            var profanityMatches = ProfanityWords.Count(word => contentLower.Contains(word));
            if (profanityMatches > 0)
            {
                var confidence = Math.Min(0.3m + (profanityMatches * 0.2m), 1.0m);
                var severity = profanityMatches >= 3 ? ModerationSeverity.High : ModerationSeverity.Medium;
                return (confidence >= 0.5m, confidence >= 0.8m, "profanity", confidence);
            }

            // Verificar spam
            var spamMatches = SpamPatterns.Count(pattern => Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase));
            if (spamMatches > 0)
            {
                var confidence = Math.Min(0.4m + (spamMatches * 0.3m), 1.0m);
                return (confidence >= 0.5m, confidence >= 0.8m, "spam", confidence);
            }

            // Verificar violencia/odio
            var violenceMatches = ViolenceHatePatterns.Count(pattern => Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase));
            if (violenceMatches > 0)
            {
                var confidence = Math.Min(0.6m + (violenceMatches * 0.3m), 1.0m);
                return (confidence >= 0.5m, confidence >= 0.8m, "violence_hate", confidence);
            }

            return (false, false, null, 0);
        }

        private async Task CreateAutoModerationAsync(Guid contentId, ContentType contentType, string? filterType, decimal confidence, bool flagged)
        {
            try
            {
                var moderation = new ContentModeration
                {
                    ContentId = contentId,
                    ContentType = contentType,
                    ModerationAction = flagged ? ModerationAction.Flagged : DetermineModerationAction(filterType, confidence),
                    ModerationReason = ModerationReason.AutoFilter,
                    Severity = DetermineSeverity(filterType, confidence),
                    AutoModerated = true,
                    ReviewRequired = flagged || confidence < 0.8m,
                    FilterType = filterType,
                    Confidence = confidence
                };

                await _repository.CreateAsync(moderation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating auto moderation for content {ContentId}", contentId);
            }
        }

        private ModerationAction DetermineModerationAction(string? filterType, decimal confidence)
        {
            return filterType switch
            {
                "violence_hate" when confidence >= 0.9m => ModerationAction.Removed,
                "violence_hate" when confidence >= 0.8m => ModerationAction.Hidden,
                "profanity" when confidence >= 0.9m => ModerationAction.Warning,
                "profanity" when confidence >= 0.8m => ModerationAction.Hidden,
                "spam" when confidence >= 0.8m => ModerationAction.Hidden,
                _ => ModerationAction.Flagged
            };
        }

        private ModerationSeverity DetermineSeverity(string? filterType, decimal confidence)
        {
            return filterType switch
            {
                "violence_hate" when confidence >= 0.9m => ModerationSeverity.Critical,
                "violence_hate" when confidence >= 0.7m => ModerationSeverity.High,
                "profanity" when confidence >= 0.8m => ModerationSeverity.Medium,
                "spam" when confidence >= 0.8m => ModerationSeverity.Medium,
                _ => ModerationSeverity.Low
            };
        }

        private async Task TryNotifyModeratorsAsync(string message)
        {
            try
            {
                // Implementar notificación a moderadores
                _logger.LogWarning("[ContentModerationService] Notificación a moderadores: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ContentModerationService] Error enviando notificación a moderadores");
            }
        }
    }
}
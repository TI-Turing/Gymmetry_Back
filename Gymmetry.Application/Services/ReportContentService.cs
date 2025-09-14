using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.ReportContent;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Cache;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Application.Services
{
    public class ReportContentService : IReportContentService
    {
        private readonly IReportContentRepository _repository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IRedisCacheService _redis;
        private readonly IReportContentRealtimeService _realtime;
        private readonly IBlobStorageService _blobStorage;
        private readonly ILogger<ReportContentService> _logger;
        
        private const string PendingCacheKey = "reports:pending";
        private const string StatsCacheKey = "reports:stats";
        private const string UserCachePrefix = "reports:user:";
        private const string AuditContainer = "report-audit";
        private const string BackupContainer = "report-backup";

        private static readonly TimeSpan PendingTtl = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan StatsTtl = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan UserCacheTtl = TimeSpan.FromMinutes(2);
        private const int AutoFlagThreshold = 5;

        public ReportContentService(
            IReportContentRepository repository, 
            ILogChangeService logChangeService, 
            ILogErrorService logErrorService, 
            INotificationService notificationService, 
            IMapper mapper, 
            IRedisCacheService redis, 
            IReportContentRealtimeService realtime, 
            IBlobStorageService blobStorage,
            ILogger<ReportContentService> logger)
        {
            _repository = repository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _notificationService = notificationService;
            _mapper = mapper;
            _redis = redis;
            _realtime = realtime;
            _blobStorage = blobStorage;
            _logger = logger;
        }

        public async Task<ApplicationResponse<ReportContentResponse>> CreateAsync(ReportContentCreateRequest request, Guid? userId, string ip = "", string invocationId = "")
        {
            _logger.LogInformation("[ReportContentService] Inicio CreateAsync");
            
            try
            {
                if (request == null)
                    return ApplicationResponse<ReportContentResponse>.ErrorResponse("Request nulo", "BadRequest");

                // Verificar duplicados
                if (await _repository.ExistsDuplicateAsync(request.ReporterId, request.ReportedContentId, request.ContentType))
                    return ApplicationResponse<ReportContentResponse>.ErrorResponse("Ya existe un reporte de este usuario para este contenido.", "Duplicate");

                var entity = _mapper.Map<ReportContent>(request);
                entity.Ip = ip;
                entity.Priority = ComputeBasePriority(entity.Reason);
                entity.CreatedAt = DateTime.UtcNow;
                entity.IsActive = true;

                var created = await _repository.CreateAsync(entity);
                
                // Auto-escalado por cantidad de reportes
                var count = await _repository.CountForContentAsync(created.ReportedContentId, created.ContentType);
                var escalated = EscalateByCount(created.Priority, count);
                if (escalated != created.Priority)
                {
                    created.Priority = escalated;
                    await _repository.UpdateAsync(created);
                }

                var respObj = _mapper.Map<ReportContentResponse>(created);
                
                // Invalidar caches
                await InvalidateCachesAsync(created.ReporterId, created.ReportedUserId);
                
                // Notificaciones
                await TryNotifyAsync(created.ReportedUserId, "content_reported", $"Tu contenido ha sido reportado por {created.Reason}.");
                
                if (count >= AutoFlagThreshold)
                {
                    await TryNotifyAsync(created.ReportedUserId, "content_autoflag", $"Tu contenido alcanzó {count} reportes.");
                }

                // Realtime y auditoría
                await _realtime.ReportCreatedAsync(respObj);
                await WriteAuditAsync(created, "created");
                await TryUploadBackupAsync(created, "created");

                _logger.LogInformation($"[ReportContentService] ReportContent creado: {created.Id}");
                return ApplicationResponse<ReportContentResponse>.SuccessResponse(respObj, "Reporte creado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en CreateAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<ReportContentResponse>.ErrorResponse("Error técnico al crear reporte", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<ReportContentResponse>> GetByIdAsync(Guid id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    return ApplicationResponse<ReportContentResponse>.ErrorResponse("Reporte no encontrado.", "NotFound");
                }

                var response = _mapper.Map<ReportContentResponse>(entity);
                return ApplicationResponse<ReportContentResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en GetByIdAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<ReportContentResponse>.ErrorResponse("Error técnico al consultar reporte", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<ReportContentListResponse>> GetPagedAsync(int page, int pageSize)
        {
            try
            {
                var list = await _repository.GetPagedAsync(page, pageSize);
                var total = await _repository.CountAsync();
                
                var resp = new ReportContentListResponse
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalItems = total,
                    TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                    Items = list.Select(l => _mapper.Map<ReportContentResponse>(l)).ToList()
                };
                
                return ApplicationResponse<ReportContentListResponse>.SuccessResponse(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en GetPagedAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<ReportContentListResponse>.ErrorResponse("Error técnico al consultar reportes", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> UpdateAsync(ReportContentUpdateRequest request, Guid? userId, string ip = "", string invocationId = "")
        {
            try
            {
                var before = await _repository.GetByIdAsync(request.Id);
                if (before == null)
                    return ApplicationResponse<bool>.ErrorResponse("Reporte no encontrado.", "NotFound");

                var entity = _mapper.Map<ReportContent>(request);
                entity.Id = request.Id;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.ReviewedAt = (request.Status == ReportStatus.Resolved || request.Status == ReportStatus.Dismissed || request.Status == ReportStatus.UnderReview) ? DateTime.UtcNow : before.ReviewedAt;
                
                var updated = await _repository.UpdateAsync(entity);
                if (updated)
                {
                    await _logChangeService.LogChangeAsync("ReportContent", before, userId, ip, invocationId);
                    await InvalidateCachesAsync(before.ReporterId, before.ReportedUserId);
                    
                    // Auditoría según estado
                    if (entity.Status == ReportStatus.Resolved)
                    {
                        await WriteAuditAsync(entity, "resolved");
                        await TryUploadBackupAsync(entity, "resolved");
                    }
                    else if (entity.Status == ReportStatus.Dismissed)
                    {
                        await WriteAuditAsync(entity, "dismissed");
                        await TryUploadBackupAsync(entity, "dismissed");
                    }
                    else
                    {
                        await WriteAuditAsync(entity, "updated");
                    }

                    await _realtime.ReportUpdatedAsync(_mapper.Map<ReportContentResponse>(entity));
                    await TryNotifyAsync(before.ReporterId, "report_status", $"Tu reporte {before.Id} ahora está en estado {request.Status}.");
                    
                    return ApplicationResponse<bool>.SuccessResponse(true, "Reporte actualizado correctamente.");
                }
                
                return ApplicationResponse<bool>.ErrorResponse("No se pudo actualizar el reporte (no encontrado o inactivo).", "NotFound");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en UpdateAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al actualizar reporte", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                var deleted = await _repository.DeleteAsync(id);
                
                if (deleted)
                {
                    await InvalidateCachesAsync(existing?.ReporterId, existing?.ReportedUserId);
                    return ApplicationResponse<bool>.SuccessResponse(true, "Reporte eliminado correctamente.");
                }
                
                return ApplicationResponse<bool>.ErrorResponse("Reporte no encontrado o ya eliminado.", "NotFound");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en DeleteAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al eliminar reporte", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<ReportContentResponse>>> FindByFieldsAsync(Dictionary<string, object> filters)
        {
            try
            {
                var list = await _repository.FindByFieldsAsync(filters);
                var mapped = list.Select(l => _mapper.Map<ReportContentResponse>(l));
                return ApplicationResponse<IEnumerable<ReportContentResponse>>.SuccessResponse(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en FindByFieldsAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<ReportContentResponse>>.ErrorResponse("Error técnico al buscar reportes", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<ReportContentResponse>>> GetPendingAsync()
        {
            try
            {
                var cached = await _redis.GetAsync(PendingCacheKey);
                if (cached != null)
                {
                    var dto = JsonSerializer.Deserialize<List<ReportContentResponse>>(cached) ?? new();
                    return ApplicationResponse<IEnumerable<ReportContentResponse>>.SuccessResponse(dto);
                }

                var list = await _repository.GetPendingAsync();
                var mapped = list.Select(l => _mapper.Map<ReportContentResponse>(l)).ToList();
                await _redis.SetAsync(PendingCacheKey, JsonSerializer.Serialize(mapped), PendingTtl);
                
                return ApplicationResponse<IEnumerable<ReportContentResponse>>.SuccessResponse(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en GetPendingAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<ReportContentResponse>>.ErrorResponse("Error técnico al obtener reportes pendientes", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<ReportContentResponse>> ReviewAsync(Guid id, Guid reviewerId, string? resolution, bool dismiss, bool resolve)
        {
            try
            {
                var updated = await _repository.MarkReviewedAsync(id, reviewerId, resolution, dismiss, resolve);
                if (updated == null)
                    return ApplicationResponse<ReportContentResponse>.ErrorResponse("Reporte no encontrado.", "NotFound");

                await InvalidateCachesAsync(updated.ReporterId, updated.ReportedUserId);
                
                if (updated.Status == ReportStatus.Resolved)
                {
                    await WriteAuditAsync(updated, "resolved");
                    await TryUploadBackupAsync(updated, "resolved");
                }
                else if (updated.Status == ReportStatus.Dismissed)
                {
                    await WriteAuditAsync(updated, "dismissed");
                    await TryUploadBackupAsync(updated, "dismissed");
                }
                else
                {
                    await WriteAuditAsync(updated, "reviewed");
                }

                await _realtime.ReportReviewedAsync(_mapper.Map<ReportContentResponse>(updated));
                await TryNotifyAsync(updated.ReporterId, "report_status", $"Tu reporte {updated.Id} ahora está en estado {updated.Status}.");
                
                return ApplicationResponse<ReportContentResponse>.SuccessResponse(_mapper.Map<ReportContentResponse>(updated), "Reporte revisado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en ReviewAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<ReportContentResponse>.ErrorResponse("Error técnico al revisar reporte", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<ReportContentStatsResponse>> GetStatsAsync()
        {
            try
            {
                var cached = await _redis.GetAsync(StatsCacheKey);
                if (cached != null)
                {
                    var dto = JsonSerializer.Deserialize<ReportContentStatsResponse>(cached);
                    if (dto != null) return ApplicationResponse<ReportContentStatsResponse>.SuccessResponse(dto);
                }

                var stats = await _repository.GetStatsAsync();
                var resp = new ReportContentStatsResponse
                {
                    Total = stats.total,
                    Pending = stats.pending,
                    UnderReview = stats.underReview,
                    Resolved = stats.resolved,
                    Dismissed = stats.dismissed,
                    ByReason = stats.byReason,
                    ByPriority = stats.byPriority,
                    ByContentType = stats.byType
                };

                await _redis.SetAsync(StatsCacheKey, JsonSerializer.Serialize(resp), StatsTtl);
                await _realtime.StatsChangedAsync();
                
                return ApplicationResponse<ReportContentStatsResponse>.SuccessResponse(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en GetStatsAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<ReportContentStatsResponse>.ErrorResponse("Error técnico al obtener estadísticas", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<ReportContentResponse>>> GetByUserAsync(Guid userId)
        {
            try
            {
                var cacheKey = UserCachePrefix + userId;
                var cached = await _redis.GetAsync(cacheKey);
                if (cached != null)
                {
                    var list = JsonSerializer.Deserialize<List<ReportContentResponse>>(cached) ?? new();
                    return ApplicationResponse<IEnumerable<ReportContentResponse>>.SuccessResponse(list);
                }

                var filters = new Dictionary<string, object> { { nameof(ReportContent.ReporterId), userId } };
                var data = await _repository.FindByFieldsAsync(filters);
                var mapped = data.Select(d => _mapper.Map<ReportContentResponse>(d)).ToList();
                
                await _redis.SetAsync(cacheKey, JsonSerializer.Serialize(mapped), UserCacheTtl);
                return ApplicationResponse<IEnumerable<ReportContentResponse>>.SuccessResponse(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en GetByUserAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<ReportContentResponse>>.ErrorResponse("Error técnico al consultar reportes de usuario", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<ReportContentAudit>>> GetAuditsAsync(Guid reportId)
        {
            try
            {
                var audits = await _repository.GetAuditsAsync(reportId);
                return ApplicationResponse<IEnumerable<ReportContentAudit>>.SuccessResponse(audits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReportContentService] Error en GetAuditsAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<ReportContentAudit>>.ErrorResponse("Error técnico al obtener auditorías", "TechnicalError");
            }
        }

        private async Task InvalidateCachesAsync(Guid? reporterId, Guid? reportedUserId)
        {
            try { await _redis.RemoveAsync(PendingCacheKey); } catch { }
            try { await _redis.RemoveAsync(StatsCacheKey); } catch { }
            if (reporterId.HasValue)
            {
                try { await _redis.RemoveAsync(UserCachePrefix + reporterId.Value); } catch { }
            }
            if (reportedUserId.HasValue)
            {
                try { await _redis.RemoveAsync(UserCachePrefix + reportedUserId.Value); } catch { }
            }
        }

        private async Task TryNotifyAsync(Guid? userId, string code, string message)
        {
            if (!userId.HasValue || userId == Guid.Empty) return;
            try
            {
                await _notificationService.CreateNotificationAsync(new Domain.DTO.Notification.Request.NotificationCreateRequestDto
                {
                    UserId = userId.Value,
                    Title = code,
                    Body = message
                });
            }
            catch { }
        }

        private async Task WriteAuditAsync(ReportContent entity, string action)
        {
            try
            {
                var snapshotObj = new
                {
                    entity.Id,
                    entity.ReportedContentId,
                    entity.ContentType,
                    entity.Status,
                    entity.Priority,
                    entity.Reason,
                    entity.ReviewedBy,
                    entity.ReviewedAt,
                    entity.Resolution,
                    entity.CreatedAt,
                    entity.UpdatedAt,
                    action,
                    At = DateTime.UtcNow
                };
                var snapshot = JsonSerializer.Serialize(snapshotObj);
                
                await _repository.AddAuditAsync(new ReportContentAudit
                {
                    ReportContentId = entity.Id,
                    Action = action,
                    SnapshotJson = snapshot,
                    ActorUserId = entity.ReviewedBy,
                    Ip = entity.Ip
                });

                // Subir snapshot a Blob (fire-and-forget)
                _ = _blobStorage.UploadTextAsync(AuditContainer, $"{entity.Id}/{DateTime.UtcNow:yyyyMMddHHmmssfff}_{action}.json", snapshot);
            }
            catch { }
        }

        private async Task TryUploadBackupAsync(ReportContent entity, string action)
        {
            try
            {
                if (action is not ("resolved" or "dismissed" or "created")) return;
                
                var backupObj = new
                {
                    entity.Id,
                    entity.ReportedContentId,
                    entity.ContentType,
                    entity.ReporterId,
                    entity.ReportedUserId,
                    entity.Reason,
                    entity.Description,
                    entity.Status,
                    entity.Priority,
                    entity.ReviewedBy,
                    entity.ReviewedAt,
                    entity.Resolution,
                    entity.CreatedAt,
                    entity.UpdatedAt,
                    BackupAction = action,
                    BackupAt = DateTime.UtcNow
                };
                
                var json = JsonSerializer.Serialize(backupObj);
                await _blobStorage.UploadTextAsync(BackupContainer, $"{entity.Id}/{DateTime.UtcNow:yyyyMMddHHmmssfff}_{action}_backup.json", json);
            }
            catch { }
        }

        private ReportPriority ComputeBasePriority(ReportReason reason)
        {
            return reason switch
            {
                ReportReason.Violence => ReportPriority.High,
                ReportReason.Hate => ReportPriority.High,
                ReportReason.Harassment => ReportPriority.High,
                ReportReason.Misinformation => ReportPriority.Medium,
                ReportReason.InappropriateContent => ReportPriority.Medium,
                ReportReason.Spam => ReportPriority.Low,
                _ => ReportPriority.Low
            };
        }

        private ReportPriority EscalateByCount(ReportPriority current, int count)
        {
            if (count >= 20) return ReportPriority.Critical;
            if (count >= 10 && current < ReportPriority.High) return ReportPriority.High;
            if (count >= 5 && current < ReportPriority.Medium) return ReportPriority.Medium;
            return current;
        }
    }
}

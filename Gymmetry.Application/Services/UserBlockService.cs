using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.UserBlock;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Application.Services
{
    public class UserBlockService : IUserBlockService
    {
        private readonly IUserBlockRepository _repository;
        private readonly IUserRepository _userRepository;
        private readonly ILogChangeService _logChangeService;
        private readonly ILogErrorService _logErrorService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserBlockService> _logger;

        private const int MaxBlocksPerDay = 20;

        public UserBlockService(
            IUserBlockRepository repository,
            IUserRepository userRepository,
            ILogChangeService logChangeService,
            ILogErrorService logErrorService,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<UserBlockService> logger)
        {
            _repository = repository;
            _userRepository = userRepository;
            _logChangeService = logChangeService;
            _logErrorService = logErrorService;
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApplicationResponse<UserBlockResponse>> BlockUserAsync(UserBlockCreateRequest request, Guid blockerId, string ip = "", string invocationId = "")
        {
            _logger.LogInformation("[UserBlockService] Inicio BlockUserAsync - Blocker: {BlockerId}, Blocked: {BlockedUserId}", blockerId, request.BlockedUserId);

            try
            {
                // Validación: No auto-bloqueo
                if (blockerId == request.BlockedUserId)
                {
                    return ApplicationResponse<UserBlockResponse>.ErrorResponse("No puedes bloquearte a ti mismo.", "SelfBlock");
                }

                // Validación: Usuario existe y está activo
                var targetUser = await _userRepository.GetUserByIdAsync(request.BlockedUserId);
                if (targetUser == null || targetUser.IsActive != true)
                {
                    return ApplicationResponse<UserBlockResponse>.ErrorResponse("Usuario no encontrado o inactivo.", "UserNotFound");
                }

                // Validación: No bloqueo duplicado
                if (await _repository.ExistsAsync(blockerId, request.BlockedUserId))
                {
                    return ApplicationResponse<UserBlockResponse>.ErrorResponse("Usuario ya está bloqueado.", "AlreadyBlocked");
                }

                // Validación: Límite diario
                var blocksToday = await _repository.CountBlocksByUserTodayAsync(blockerId);
                if (blocksToday >= MaxBlocksPerDay)
                {
                    return ApplicationResponse<UserBlockResponse>.ErrorResponse($"Has alcanzado el límite de {MaxBlocksPerDay} bloqueos por día.", "DailyLimitReached");
                }

                // Crear bloqueo
                var userBlock = new UserBlock
                {
                    BlockerId = blockerId,
                    BlockedUserId = request.BlockedUserId,
                    Reason = request.Reason,
                    Ip = ip
                };

                var created = await _repository.CreateAsync(userBlock);
                var response = _mapper.Map<UserBlockResponse>(created);

                // Log de cambio
                await _logChangeService.LogChangeAsync("UserBlock", null, blockerId, ip, invocationId);

                // Notificación a moderadores si es bloqueo masivo
                if (blocksToday >= 10)
                {
                    await TryNotifyModeratorsAsync($"Usuario {blockerId} ha realizado {blocksToday + 1} bloqueos hoy - Posible comportamiento abusivo");
                }

                _logger.LogInformation("[UserBlockService] Usuario {BlockerId} bloqueó a {BlockedUserId}", blockerId, request.BlockedUserId);
                return ApplicationResponse<UserBlockResponse>.SuccessResponse(response, "Usuario bloqueado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserBlockService] Error en BlockUserAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<UserBlockResponse>.ErrorResponse("Error técnico al bloquear usuario", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> UnblockUserAsync(Guid blockerId, Guid blockedUserId)
        {
            _logger.LogInformation("[UserBlockService] Inicio UnblockUserAsync - Blocker: {BlockerId}, Blocked: {BlockedUserId}", blockerId, blockedUserId);

            try
            {
                var deleted = await _repository.DeleteAsync(blockerId, blockedUserId);
                
                if (!deleted)
                {
                    return ApplicationResponse<bool>.ErrorResponse("Bloqueo no encontrado.", "NotFound");
                }

                _logger.LogInformation("[UserBlockService] Usuario {BlockerId} desbloqueó a {BlockedUserId}", blockerId, blockedUserId);
                return ApplicationResponse<bool>.SuccessResponse(true, "Usuario desbloqueado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserBlockService] Error en UnblockUserAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al desbloquear usuario", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<UserBlockResponse>>> GetBlockedUsersAsync(Guid userId, int page = 1, int pageSize = 50)
        {
            try
            {
                var userBlocks = await _repository.GetBlockedByUserAsync(userId, page, pageSize);
                var responses = userBlocks.Select(ub => _mapper.Map<UserBlockResponse>(ub));
                return ApplicationResponse<IEnumerable<UserBlockResponse>>.SuccessResponse(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserBlockService] Error en GetBlockedUsersAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<UserBlockResponse>>.ErrorResponse("Error técnico al obtener usuarios bloqueados", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<UserBlockResponse>>> GetBlockersAsync(Guid userId, int page = 1, int pageSize = 50)
        {
            try
            {
                var userBlocks = await _repository.GetBlockersOfUserAsync(userId, page, pageSize);
                var responses = userBlocks.Select(ub => _mapper.Map<UserBlockResponse>(ub));
                return ApplicationResponse<IEnumerable<UserBlockResponse>>.SuccessResponse(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserBlockService] Error en GetBlockersAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<UserBlockResponse>>.ErrorResponse("Error técnico al obtener bloqueadores", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<UserBlockCheckResponse>> CheckBlockStatusAsync(Guid userId, Guid targetUserId)
        {
            try
            {
                var userBlock = await _repository.GetBlockBetweenUsersAsync(userId, targetUserId);
                
                var response = new UserBlockCheckResponse
                {
                    IsBlocked = userBlock != null,
                    IsMutual = false,
                    BlockedAt = userBlock?.CreatedAt,
                    Reason = userBlock?.Reason
                };

                if (userBlock != null)
                {
                    // Verificar si es mutuo
                    var reverseBlock = await _repository.ExistsAsync(targetUserId, userId);
                    response.IsMutual = reverseBlock;
                }

                return ApplicationResponse<UserBlockCheckResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserBlockService] Error en CheckBlockStatusAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<UserBlockCheckResponse>.ErrorResponse("Error técnico al verificar estado de bloqueo", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<UserBlockResponse>>> FindByFieldsAsync(Dictionary<string, object> filters)
        {
            try
            {
                var userBlocks = await _repository.FindByFieldsAsync(filters);
                var responses = userBlocks.Select(ub => _mapper.Map<UserBlockResponse>(ub));
                return ApplicationResponse<IEnumerable<UserBlockResponse>>.SuccessResponse(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserBlockService] Error en FindByFieldsAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<UserBlockResponse>>.ErrorResponse("Error técnico al buscar bloqueos", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<UserBlockStatsResponse>> GetStatsAsync(Guid userId)
        {
            try
            {
                var stats = new UserBlockStatsResponse
                {
                    TotalBlocks = await _repository.CountTotalBlocksAsync(),
                    BlockedByMe = await _repository.CountBlockedByUserAsync(userId),
                    BlockingMe = await _repository.CountBlockersOfUserAsync(userId),
                    MutualBlocks = await _repository.CountMutualBlocksAsync(userId),
                    TodayBlocks = await _repository.CountBlocksByUserTodayAsync(userId),
                    BlocksByDay = await _repository.GetBlocksStatsLast7DaysAsync(userId)
                };

                return ApplicationResponse<UserBlockStatsResponse>.SuccessResponse(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserBlockService] Error en GetStatsAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<UserBlockStatsResponse>.ErrorResponse("Error técnico al obtener estadísticas", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<BulkUnblockResponse>> BulkUnblockAsync(BulkUnblockRequest request, Guid userId)
        {
            try
            {
                var response = new BulkUnblockResponse
                {
                    TotalRequested = request.UserIds.Count
                };

                if (!request.UserIds.Any())
                {
                    response.Errors.Add("No se proporcionaron usuarios para desbloquear");
                    return ApplicationResponse<BulkUnblockResponse>.SuccessResponse(response);
                }

                // Validar límite de operaciones bulk (máximo 50)
                if (request.UserIds.Count > 50)
                {
                    return ApplicationResponse<BulkUnblockResponse>.ErrorResponse("Máximo 50 usuarios por operación bulk", "BulkLimitExceeded");
                }

                var success = await _repository.BulkUnblockAsync(userId, request.UserIds);
                
                if (success)
                {
                    response.SuccessfullyUnblocked = request.UserIds.Count;
                }
                else
                {
                    response.FailedUnblocks.AddRange(request.UserIds);
                    response.Errors.Add("No se encontraron bloqueos activos para los usuarios especificados");
                }

                _logger.LogInformation("[UserBlockService] Bulk unblock - Usuario {UserId}: {Success}/{Total}", 
                    userId, response.SuccessfullyUnblocked, response.TotalRequested);

                return ApplicationResponse<BulkUnblockResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserBlockService] Error en BulkUnblockAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<BulkUnblockResponse>.ErrorResponse("Error técnico en operación bulk", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<UserBlockResponse>>> GetMutualBlocksAsync(Guid userId, int page = 1, int pageSize = 50)
        {
            try
            {
                var userBlocks = await _repository.GetMutualBlocksAsync(userId, page, pageSize);
                var responses = userBlocks.Select(ub => _mapper.Map<UserBlockResponse>(ub));
                return ApplicationResponse<IEnumerable<UserBlockResponse>>.SuccessResponse(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserBlockService] Error en GetMutualBlocksAsync");
                await _logErrorService.LogErrorAsync(ex);
                return ApplicationResponse<IEnumerable<UserBlockResponse>>.ErrorResponse("Error técnico al obtener bloqueos mutuos", "TechnicalError");
            }
        }

        private async Task TryNotifyModeratorsAsync(string message)
        {
            try
            {
                // Aquí implementarías la lógica para notificar a moderadores
                // Por ejemplo, crear notificaciones para usuarios con rol moderador
                _logger.LogWarning("[UserBlockService] Notificación a moderadores: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UserBlockService] Error enviando notificación a moderadores");
            }
        }
    }
}
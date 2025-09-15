using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.PostShare.Request;
using Gymmetry.Domain.DTO.PostShare.Response;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Cache;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Gymmetry.Application.Services
{
    public class PostShareService : IPostShareService
    {
        private readonly IPostShareRepository _postShareRepository;
        private readonly IPostRepository _postRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRedisCacheService _redis;
        private readonly IMapper _mapper;
        private readonly ILogger<PostShareService> _logger;

        private const string CountersCachePrefix = "postshare:cnt:";
        private const string RateLimitPrefix = "postshare:rate:";
        private const int RateLimitMaxShares = 60; // 60 shares por hora
        private static readonly TimeSpan CountersCacheTtl = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan RateLimitTtl = TimeSpan.FromHours(1);

        private static readonly HashSet<string> ValidPlatforms = new()
        {
            "App", "WhatsApp", "Instagram", "Facebook", "Twitter", "SMS", "Email", "Other"
        };

        private static readonly HashSet<string> ValidShareTypes = new()
        {
            "Internal", "External"
        };

        public PostShareService(
            IPostShareRepository postShareRepository,
            IPostRepository postRepository,
            IUserRepository userRepository,
            IRedisCacheService redis,
            IMapper mapper,
            ILogger<PostShareService> logger)
        {
            _postShareRepository = postShareRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;  
            _redis = redis;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApplicationResponse<PostShareResponse>> CreatePostShareAsync(AddPostShareRequest request, string ip = "")
        {
            try
            {
                // Validaciones de negocio
                var validationResult = await ValidateCreateRequestAsync(request);
                if (!validationResult.Success)
                    return ApplicationResponse<PostShareResponse>.ErrorResponse(validationResult.Message, "ValidationError");

                // Rate limiting
                var rateLimitResult = await CheckRateLimitAsync(request.SharedBy);
                if (!rateLimitResult)
                    return ApplicationResponse<PostShareResponse>.ErrorResponse("Rate limit excedido. Máximo 60 compartidos por hora.", "RateLimitExceeded");

                // Crear entidad
                var postShare = new PostShare
                {
                    PostId = request.PostId,
                    SharedBy = request.SharedBy,
                    SharedWith = request.SharedWith,
                    ShareType = NormalizePlatform(request.ShareType),
                    Platform = NormalizePlatform(request.Platform),
                    Metadata = request.Metadata,
                    Ip = ip
                };

                var created = await _postShareRepository.CreatePostShareAsync(postShare);

                // Invalidar cache de contadores
                await InvalidateCountersCacheAsync(request.PostId);

                // Incrementar rate limit
                await IncrementRateLimitAsync(request.SharedBy);

                // Log estructurado
                _logger.LogInformation("PostShare created: {UserId} shared {PostId} via {Platform} ({ShareType})", 
                    request.SharedBy, request.PostId, request.Platform, request.ShareType);

                var response = _mapper.Map<PostShareResponse>(created);
                return ApplicationResponse<PostShareResponse>.SuccessResponse(response, "Post compartido exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear PostShare");
                return ApplicationResponse<PostShareResponse>.ErrorResponse("Error técnico al compartir el post.");
            }
        }

        public async Task<ApplicationResponse<bool>> UpdatePostShareAsync(UpdatePostShareRequest request)
        {
            try
            {
                var postShare = new PostShare
                {
                    Id = request.Id,
                    Metadata = request.Metadata,
                    IsActive = request.IsActive ?? true
                };

                var updated = await _postShareRepository.UpdatePostShareAsync(postShare);
                if (!updated)
                    return ApplicationResponse<bool>.ErrorResponse("PostShare no encontrado o inactivo.", "NotFound");

                // Invalidar cache si es necesario
                var existing = await _postShareRepository.GetPostShareByIdAsync(request.Id);
                if (existing != null)
                    await InvalidateCountersCacheAsync(existing.PostId);

                return ApplicationResponse<bool>.SuccessResponse(true, "PostShare actualizado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar PostShare");
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al actualizar el PostShare.");
            }
        }

        public async Task<ApplicationResponse<bool>> DeletePostShareAsync(Guid id)
        {
            try
            {
                var existing = await _postShareRepository.GetPostShareByIdAsync(id);
                if (existing == null)
                    return ApplicationResponse<bool>.ErrorResponse("PostShare no encontrado.", "NotFound");

                var deleted = await _postShareRepository.DeletePostShareAsync(id);
                if (!deleted)
                    return ApplicationResponse<bool>.ErrorResponse("No se pudo eliminar el PostShare.", "DeleteFailed");

                // Invalidar cache
                await InvalidateCountersCacheAsync(existing.PostId);

                return ApplicationResponse<bool>.SuccessResponse(true, "PostShare eliminado correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar PostShare");
                return ApplicationResponse<bool>.ErrorResponse("Error técnico al eliminar el PostShare.");
            }
        }

        public async Task<ApplicationResponse<PostShareResponse?>> GetPostShareByIdAsync(Guid id)
        {
            try
            {
                var postShare = await _postShareRepository.GetPostShareByIdAsync(id);
                if (postShare == null)
                    return ApplicationResponse<PostShareResponse?>.ErrorResponse("PostShare no encontrado.", "NotFound");

                var response = _mapper.Map<PostShareResponse>(postShare);
                return ApplicationResponse<PostShareResponse?>.SuccessResponse(response, "PostShare encontrado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener PostShare por ID");
                return ApplicationResponse<PostShareResponse?>.ErrorResponse("Error técnico al obtener el PostShare.");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<PostShareResponse>>> FindPostSharesByFieldsAsync(Dictionary<string, object> filters)
        {
            try
            {
                _logger.LogDebug("Buscando PostShares con filtros: {Filters}", JsonSerializer.Serialize(filters));

                var postShares = await _postShareRepository.FindPostSharesByFieldsAsync(filters);
                var responses = _mapper.Map<IEnumerable<PostShareResponse>>(postShares);

                return ApplicationResponse<IEnumerable<PostShareResponse>>.SuccessResponse(responses, "PostShares encontrados.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar PostShares");
                return ApplicationResponse<IEnumerable<PostShareResponse>>.ErrorResponse("Error técnico al buscar PostShares.");
            }
        }

        public async Task<ApplicationResponse<PostShareCountersResponse>> GetPostShareCountersAsync(Guid postId)
        {
            try
            {
                // Intentar obtener desde cache
                var cacheKey = $"{CountersCachePrefix}{postId}";
                var cached = await _redis.GetAsync<PostShareCountersResponse>(cacheKey);
                if (cached != null)
                    return ApplicationResponse<PostShareCountersResponse>.SuccessResponse(cached, "Contadores obtenidos desde cache.");

                // Obtener desde BD
                var counters = await _postShareRepository.GetPostShareCountersByPostIdAsync(postId);
                
                var response = new PostShareCountersResponse
                {
                    PostId = postId,
                    TotalShares = counters.Values.Sum(),
                    InternalShares = await GetInternalSharesCountAsync(postId),
                    ExternalShares = await GetExternalSharesCountAsync(postId),
                    ByPlatform = counters,
                    LastUpdated = DateTime.UtcNow
                };

                response.InternalShares = response.ByPlatform["App"];
                response.ExternalShares = response.TotalShares - response.InternalShares;

                // Guardar en cache
                await _redis.SetAsync(cacheKey, response, CountersCacheTtl);

                return ApplicationResponse<PostShareCountersResponse>.SuccessResponse(response, "Contadores calculados.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener contadores de PostShare");
                return ApplicationResponse<PostShareCountersResponse>.ErrorResponse("Error técnico al obtener contadores.");
            }
        }

        private async Task<ApplicationResponse<bool>> ValidateCreateRequestAsync(AddPostShareRequest request)
        {
            // Validar ShareType
            if (!ValidShareTypes.Contains(request.ShareType))
                return ApplicationResponse<bool>.ErrorResponse($"ShareType inválido. Valores permitidos: {string.Join(", ", ValidShareTypes)}");

            // Validar Platform
            if (!ValidPlatforms.Contains(request.Platform))
                return ApplicationResponse<bool>.ErrorResponse($"Platform inválido. Valores permitidos: {string.Join(", ", ValidPlatforms)}");

            // Validar ShareType=Internal requiere SharedWith
            if (request.ShareType == "Internal" && !request.SharedWith.HasValue)
                return ApplicationResponse<bool>.ErrorResponse("ShareType 'Internal' requiere especificar SharedWith.");

            // Validar SharedWith != SharedBy
            if (request.SharedWith.HasValue && request.SharedWith.Value == request.SharedBy)
                return ApplicationResponse<bool>.ErrorResponse("No se puede compartir un post consigo mismo.");

            // Validar que el post existe
            var post = await _postRepository.GetPostByIdAsync(request.PostId);
            if (post == null)
                return ApplicationResponse<bool>.ErrorResponse("El post especificado no existe.");

            // Validar que SharedBy existe
            var sharedByUser = await _userRepository.GetUserByIdAsync(request.SharedBy);
            if (sharedByUser == null)
                return ApplicationResponse<bool>.ErrorResponse("El usuario que comparte no existe.");

            // Validar que SharedWith existe (si aplica)
            if (request.SharedWith.HasValue)
            {
                var sharedWithUser = await _userRepository.GetUserByIdAsync(request.SharedWith.Value);
                if (sharedWithUser == null)
                    return ApplicationResponse<bool>.ErrorResponse("El usuario destinatario no existe.");
            }

            // Validar JSON Metadata
            if (!string.IsNullOrEmpty(request.Metadata))
            {
                try
                {
                    JsonSerializer.Deserialize<object>(request.Metadata);
                    if (request.Metadata.Length > 65536) // 64KB max
                        return ApplicationResponse<bool>.ErrorResponse("Metadata excede el tamaño máximo de 64KB.");
                }
                catch (JsonException)
                {
                    return ApplicationResponse<bool>.ErrorResponse("Metadata debe ser un JSON válido.");
                }
            }

            return ApplicationResponse<bool>.SuccessResponse(true);
        }

        private async Task<bool> CheckRateLimitAsync(Guid userId)
        {
            try
            {
                var hour = DateTime.UtcNow.ToString("yyyyMMddHH");
                var rateLimitKey = $"{RateLimitPrefix}{userId}:{hour}";
                
                var currentString = await _redis.GetAsync(rateLimitKey);
                var current = int.TryParse(currentString, out var parsed) ? parsed : 0;
                return current < RateLimitMaxShares;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking rate limit for user {UserId}, allowing request", userId);
                return true; // En caso de error, permitir la solicitud
            }
        }

        private async Task IncrementRateLimitAsync(Guid userId)
        {
            try
            {
                var hour = DateTime.UtcNow.ToString("yyyyMMddHH");
                var rateLimitKey = $"{RateLimitPrefix}{userId}:{hour}";
                
                await _redis.IncrementAsync(rateLimitKey, RateLimitTtl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error incrementing rate limit for user {UserId}", userId);
            }
        }

        private async Task InvalidateCountersCacheAsync(Guid postId)
        {
            try
            {
                var cacheKey = $"{CountersCachePrefix}{postId}";
                await _redis.RemoveAsync(cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error invalidating cache for post {PostId}", postId);
            }
        }

        private async Task<int> GetInternalSharesCountAsync(Guid postId)
        {
            var filters = new Dictionary<string, object>
            {
                ["postid"] = postId,
                ["sharetype"] = "Internal",
                ["isactive"] = true
            };

            var shares = await _postShareRepository.FindPostSharesByFieldsAsync(filters);
            return shares.Count();
        }

        private async Task<int> GetExternalSharesCountAsync(Guid postId)
        {
            var filters = new Dictionary<string, object>
            {
                ["postid"] = postId,
                ["sharetype"] = "External",
                ["isactive"] = true
            };

            var shares = await _postShareRepository.FindPostSharesByFieldsAsync(filters);
            return shares.Count();
        }

        private static string NormalizePlatform(string platform)
        {
            return platform switch
            {
                "whatsapp" => "WhatsApp",
                "instagram" => "Instagram",
                "facebook" => "Facebook",
                "twitter" => "Twitter",  
                "sms" => "SMS",
                "email" => "Email",
                "app" => "App",
                _ => platform
            };
        }
    }
}
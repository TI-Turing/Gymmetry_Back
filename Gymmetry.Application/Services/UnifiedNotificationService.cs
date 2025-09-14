using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.UnifiedNotification;
using Gymmetry.Domain.Models;
using Gymmetry.Domain.Options;
using Gymmetry.Repository.Services.Interfaces;
using Gymmetry.Repository.Services.Cache; // Agregado para IRedisCacheService

namespace Gymmetry.Application.Services
{
    public class UnifiedNotificationService : IUnifiedNotificationService
    {
        private readonly INotificationTemplateRepository _templateRepository;
        private readonly IUserNotificationPreferenceRepository _preferenceRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationDeliveryService _deliveryService;
        private readonly IUserRepository _userRepository;
        private readonly IRedisCacheService _cache;
        private readonly ILogger<UnifiedNotificationService> _logger;

        private const int MaxNotificationsPerHour = 100;
        private const string RateLimitCacheKey = "notification:ratelimit:{0}:{1:yyyyMMddHH}";

        public UnifiedNotificationService(
            INotificationTemplateRepository templateRepository,
            IUserNotificationPreferenceRepository preferenceRepository,
            INotificationRepository notificationRepository,
            INotificationDeliveryService deliveryService,
            IUserRepository userRepository,
            IRedisCacheService cache,
            ILogger<UnifiedNotificationService> logger)
        {
            _templateRepository = templateRepository;
            _preferenceRepository = preferenceRepository;
            _notificationRepository = notificationRepository;
            _deliveryService = deliveryService;
            _userRepository = userRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ApplicationResponse<NotificationDeliveryResult>> SendUnifiedNotificationAsync(UnifiedNotificationRequest request)
        {
            _logger.LogInformation("[UnifiedNotificationService] Enviando notificación unificada - Usuario: {UserId}, Tipo: {Type}",
                request.UserId, request.NotificationType);

            try
            {
                // 1. Validar usuario
                var user = await _userRepository.GetUserByIdAsync(request.UserId);
                if (user == null || user.IsActive != true)
                {
                    return ApplicationResponse<NotificationDeliveryResult>.ErrorResponse("Usuario no encontrado o inactivo", "UserNotFound");
                }

                // 2. Validar rate limiting
                if (!request.SkipPreferences)
                {
                    var rateLimitResult = await ValidateNotificationRulesAsync(request.NotificationType, request.UserId);
                    if (!rateLimitResult.Success)
                    {
                        return ApplicationResponse<NotificationDeliveryResult>.ErrorResponse(rateLimitResult.Message, "RateLimitExceeded");
                    }
                }

                // 3. Obtener template
                var template = await _templateRepository.GetByTemplateKeyAsync(request.TemplateKey);
                if (template == null)
                {
                    return ApplicationResponse<NotificationDeliveryResult>.ErrorResponse("Template no encontrado", "TemplateNotFound");
                }

                // 4. Procesar template con datos
                var processedTitle = ProcessTemplate(template.Subject, request.TemplateData);
                var processedBody = ProcessTemplate(template.BodyTemplate, request.TemplateData);

                // 5. Crear notificación en BD
                var notification = new Notification
                {
                    UserId = request.UserId,
                    Title = processedTitle,
                    Body = processedBody,
                    NotificationType = request.NotificationType,
                    Priority = request.Priority,
                    TemplateKey = request.TemplateKey,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Seen = false,
                    Opened = false
                };

                var createdNotification = await _notificationRepository.CreateNotificationAsync(notification);

                // 6. Determinar canales de envío
                var channels = await DetermineDeliveryChannelsAsync(request, template);

                // 7. Enviar por cada canal
                var deliveryResult = new NotificationDeliveryResult
                {
                    NotificationId = createdNotification.Id,
                    Success = false,
                    ChannelResults = new Dictionary<string, bool>(),
                    Errors = new List<string>()
                };

                var anySuccess = false;

                foreach (var channel in channels)
                {
                    bool channelResult = false;

                    try
                    {
                        channelResult = channel switch
                        {
                            "push" => await SendPushForUserAsync(user, processedTitle, processedBody, request.TemplateData),
                            "email" => await SendEmailForUserAsync(user, processedTitle, ProcessTemplate(template.EmailTemplate ?? template.BodyTemplate, request.TemplateData)),
                            "sms" => await SendSmsForUserAsync(user, ProcessTemplate(template.SmsTemplate ?? processedBody, request.TemplateData)),
                            "whatsapp" => await SendWhatsAppForUserAsync(user, ProcessTemplate(template.WhatsAppTemplate ?? processedBody, request.TemplateData)),
                            _ => false
                        };

                        if (channelResult) anySuccess = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error enviando por canal {Channel} para usuario {UserId}", channel, request.UserId);
                        deliveryResult.Errors.Add($"Error en canal {channel}: {ex.Message}");
                    }

                    deliveryResult.ChannelResults[channel] = channelResult;
                }

                deliveryResult.Success = anySuccess;

                // 8. Incrementar contador de rate limiting
                if (!request.SkipPreferences)
                {
                    await IncrementRateLimitCounterAsync(request.UserId);
                }

                _logger.LogInformation("[UnifiedNotificationService] Notificación enviada - ID: {NotificationId}, Éxito: {Success}",
                    createdNotification.Id, deliveryResult.Success);

                return ApplicationResponse<NotificationDeliveryResult>.SuccessResponse(deliveryResult,
                    deliveryResult.Success ? "Notificación enviada correctamente" : "Error en el envío de notificación");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UnifiedNotificationService] Error en SendUnifiedNotificationAsync");
                return ApplicationResponse<NotificationDeliveryResult>.ErrorResponse("Error técnico al enviar notificación", "TechnicalError");
            }
        }

        private async Task<string[]> DetermineDeliveryChannelsAsync(UnifiedNotificationRequest request, NotificationTemplate template)
        {
            // Si se fuerzan canales específicos
            if (request.ForcedChannels?.Any() == true)
            {
                return request.ForcedChannels;
            }

            // Para notificaciones críticas, usar todos los canales
            if (request.Priority == "critical")
            {
                return NotificationChannelRules.CriticalChannels["critical"];
            }

            // Si se saltan preferencias, usar canales por defecto del tipo
            if (request.SkipPreferences)
            {
                return NotificationChannelRules.DefaultChannels.GetValueOrDefault(request.NotificationType, new[] { "push", "app" });
            }

            // Obtener preferencias del usuario
            var preferences = await GetUserPreferencesAsync(request.UserId);
            if (!preferences.Success || !preferences.Data.Any())
            {
                // Sin preferencias, usar canales permitidos del template
                return template.AllowedChannels.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }

            // Filtrar canales según preferencias del usuario
            var userPreference = preferences.Data.FirstOrDefault(p => p.NotificationType == request.NotificationType);
            if (userPreference == null)
            {
                return template.AllowedChannels.Split(',', StringSplitOptions.RemoveEmptyEntries);
            }

            var enabledChannels = new List<string>();
            if (userPreference.PushEnabled) enabledChannels.Add("push");
            if (userPreference.AppEnabled) enabledChannels.Add("app");
            if (userPreference.EmailEnabled) enabledChannels.Add("email");
            if (userPreference.SmsEnabled) enabledChannels.Add("sms");
            if (userPreference.WhatsAppEnabled) enabledChannels.Add("whatsapp");

            // Intersección con canales permitidos del template
            var allowedChannels = template.AllowedChannels.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return enabledChannels.Intersect(allowedChannels).ToArray();
        }

        private async Task<bool> SendPushForUserAsync(User user, string title, string body, Dictionary<string, object> metadata)
        {
            // Aquí necesitarías obtener el token push del usuario desde la BD o cache
            // Por ahora, simulamos que está en algún campo del usuario
            var pushToken = user.Ip; // Placeholder - deberías tener un campo específico para push token

            if (string.IsNullOrEmpty(pushToken)) return false;

            return await _deliveryService.SendPushNotificationAsync(pushToken, title, body, metadata);
        }

        private async Task<bool> SendEmailForUserAsync(User user, string subject, string body)
        {
            if (string.IsNullOrEmpty(user.Email)) return false;
            return await _deliveryService.SendEmailAsync(user.Email, subject, body);
        }

        private async Task<bool> SendSmsForUserAsync(User user, string message)
        {
            // Usar campo Phone en lugar de PhoneNumber
            if (string.IsNullOrEmpty(user.Phone)) return false;
            return await _deliveryService.SendSmsAsync(user.Phone, message);
        }

        private async Task<bool> SendWhatsAppForUserAsync(User user, string message)
        {
            // Usar campo Phone en lugar de PhoneNumber
            if (string.IsNullOrEmpty(user.Phone)) return false;
            return await _deliveryService.SendWhatsAppAsync(user.Phone, message);
        }

        private static string ProcessTemplate(string template, Dictionary<string, object> data)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;

            var result = template;
            foreach (var kvp in data)
            {
                var placeholder = $"{{{kvp.Key}}}";
                result = result.Replace(placeholder, kvp.Value?.ToString() ?? string.Empty);
            }

            return result;
        }

        public async Task<ApplicationResponse<bool>> ValidateNotificationRulesAsync(string notificationType, Guid userId)
        {
            try
            {
                var cacheKey = string.Format(RateLimitCacheKey, userId, DateTime.UtcNow);
                var cachedValue = await _cache.GetAsync(cacheKey);
                var currentCount = int.TryParse(cachedValue, out var count) ? count : 0;

                if (currentCount >= MaxNotificationsPerHour)
                {
                    return ApplicationResponse<bool>.ErrorResponse($"Límite de {MaxNotificationsPerHour} notificaciones por hora alcanzado", "RateLimitExceeded");
                }

                return ApplicationResponse<bool>.SuccessResponse(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validando reglas de notificación para usuario {UserId}", userId);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico validando reglas", "TechnicalError");
            }
        }

        private async Task IncrementRateLimitCounterAsync(Guid userId)
        {
            try
            {
                var cacheKey = string.Format(RateLimitCacheKey, userId, DateTime.UtcNow);
                var cachedValue = await _cache.GetAsync(cacheKey);
                var currentCount = int.TryParse(cachedValue, out var count) ? count : 0;
                await _cache.SetAsync(cacheKey, (currentCount + 1).ToString(), TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementando contador de rate limit para usuario {UserId}", userId);
            }
        }

        public async Task<ApplicationResponse<string>> ProcessTemplateAsync(string templateKey, Dictionary<string, object> data)
        {
            try
            {
                var template = await _templateRepository.GetByTemplateKeyAsync(templateKey);
                if (template == null)
                {
                    return ApplicationResponse<string>.ErrorResponse("Template no encontrado", "NotFound");
                }

                var processed = ProcessTemplate(template.BodyTemplate, data);
                return ApplicationResponse<string>.SuccessResponse(processed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando template {TemplateKey}", templateKey);
                return ApplicationResponse<string>.ErrorResponse("Error técnico procesando template", "TechnicalError");
            }
        }

        // Implementaciones restantes de la interfaz...
        public async Task<ApplicationResponse<NotificationTemplate>> CreateTemplateAsync(NotificationTemplateRequest request)
        {
            try
            {
                var template = new NotificationTemplate
                {
                    TemplateKey = request.TemplateKey,
                    NotificationType = request.NotificationType,
                    Priority = request.Priority,
                    Subject = request.Subject,
                    BodyTemplate = request.BodyTemplate,
                    EmailTemplate = request.EmailTemplate,
                    SmsTemplate = request.SmsTemplate,
                    WhatsAppTemplate = request.WhatsAppTemplate,
                    RequiresEmail = request.RequiresEmail,
                    RequiresSms = request.RequiresSms,
                    AllowedChannels = request.AllowedChannels,
                    IsActive = true
                };

                var created = await _templateRepository.CreateAsync(template);
                return ApplicationResponse<NotificationTemplate>.SuccessResponse(created, "Template creado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando template {TemplateKey}", request.TemplateKey);
                return ApplicationResponse<NotificationTemplate>.ErrorResponse("Error técnico creando template", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<NotificationTemplate>>> GetTemplatesAsync()
        {
            try
            {
                var templates = await _templateRepository.GetAllAsync();
                return ApplicationResponse<IEnumerable<NotificationTemplate>>.SuccessResponse(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo templates");
                return ApplicationResponse<IEnumerable<NotificationTemplate>>.ErrorResponse("Error técnico obteniendo templates", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<NotificationTemplate>>> GetTemplatesByTypeAsync(string notificationType)
        {
            try
            {
                var templates = await _templateRepository.GetByTypeAsync(notificationType);
                return ApplicationResponse<IEnumerable<NotificationTemplate>>.SuccessResponse(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo templates por tipo {Type}", notificationType);
                return ApplicationResponse<IEnumerable<NotificationTemplate>>.ErrorResponse("Error técnico obteniendo templates", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<NotificationTemplate>> UpdateTemplateAsync(int templateId, NotificationTemplateRequest request)
        {
            try
            {
                var template = await _templateRepository.GetByIdAsync(templateId);
                if (template == null)
                {
                    return ApplicationResponse<NotificationTemplate>.ErrorResponse("Template no encontrado", "NotFound");
                }

                template.TemplateKey = request.TemplateKey;
                template.NotificationType = request.NotificationType;
                template.Priority = request.Priority;
                template.Subject = request.Subject;
                template.BodyTemplate = request.BodyTemplate;
                template.EmailTemplate = request.EmailTemplate;
                template.SmsTemplate = request.SmsTemplate;
                template.WhatsAppTemplate = request.WhatsAppTemplate;
                template.RequiresEmail = request.RequiresEmail;
                template.RequiresSms = request.RequiresSms;
                template.AllowedChannels = request.AllowedChannels;

                var updated = await _templateRepository.UpdateAsync(template);
                if (updated)
                {
                    return ApplicationResponse<NotificationTemplate>.SuccessResponse(template, "Template actualizado correctamente");
                }

                return ApplicationResponse<NotificationTemplate>.ErrorResponse("No se pudo actualizar el template", "UpdateFailed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando template {TemplateId}", templateId);
                return ApplicationResponse<NotificationTemplate>.ErrorResponse("Error técnico actualizando template", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> DeleteTemplateAsync(int templateId)
        {
            try
            {
                var deleted = await _templateRepository.DeleteAsync(templateId);
                if (deleted)
                {
                    return ApplicationResponse<bool>.SuccessResponse(true, "Template eliminado correctamente");
                }

                return ApplicationResponse<bool>.ErrorResponse("Template no encontrado", "NotFound");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando template {TemplateId}", templateId);
                return ApplicationResponse<bool>.ErrorResponse("Error técnico eliminando template", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<IEnumerable<UserNotificationPreference>>> GetUserPreferencesAsync(Guid userId)
        {
            try
            {
                var preferences = await _preferenceRepository.GetByUserAsync(userId);
                return ApplicationResponse<IEnumerable<UserNotificationPreference>>.SuccessResponse(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo preferencias de usuario {UserId}", userId);
                return ApplicationResponse<IEnumerable<UserNotificationPreference>>.ErrorResponse("Error técnico obteniendo preferencias", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<UserNotificationPreference>> UpdateUserPreferencesAsync(UserPreferencesRequest request)
        {
            try
            {
                var existing = await _preferenceRepository.GetByUserAndTypeAsync(request.UserId, request.NotificationType);
                
                if (existing == null)
                {
                    // Crear nueva preferencia
                    var newPreference = new UserNotificationPreference
                    {
                        UserId = request.UserId,
                        NotificationType = request.NotificationType,
                        PushEnabled = request.PushEnabled,
                        EmailEnabled = request.EmailEnabled,
                        SmsEnabled = request.SmsEnabled,
                        WhatsAppEnabled = request.WhatsAppEnabled,
                        AppEnabled = request.AppEnabled
                    };

                    var created = await _preferenceRepository.CreateAsync(newPreference);
                    return ApplicationResponse<UserNotificationPreference>.SuccessResponse(created, "Preferencias creadas correctamente");
                }
                else
                {
                    // Actualizar existente
                    existing.PushEnabled = request.PushEnabled;
                    existing.EmailEnabled = request.EmailEnabled;
                    existing.SmsEnabled = request.SmsEnabled;
                    existing.WhatsAppEnabled = request.WhatsAppEnabled;
                    existing.AppEnabled = request.AppEnabled;

                    var updated = await _preferenceRepository.UpdateAsync(existing);
                    if (updated)
                    {
                        return ApplicationResponse<UserNotificationPreference>.SuccessResponse(existing, "Preferencias actualizadas correctamente");
                    }

                    return ApplicationResponse<UserNotificationPreference>.ErrorResponse("No se pudieron actualizar las preferencias", "UpdateFailed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando preferencias de usuario {UserId}", request.UserId);
                return ApplicationResponse<UserNotificationPreference>.ErrorResponse("Error técnico actualizando preferencias", "TechnicalError");
            }
        }

        // Métodos de envío directo por canal
        public async Task<ApplicationResponse<bool>> SendPushNotificationAsync(ChannelDeliveryRequest request)
        {
            try
            {
                var result = await _deliveryService.SendPushNotificationAsync(request.RecipientId, request.Subject, request.Message, request.Metadata);
                return ApplicationResponse<bool>.SuccessResponse(result, result ? "Push notification enviada" : "Error enviando push notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando push notification directa");
                return ApplicationResponse<bool>.ErrorResponse("Error técnico enviando push", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> SendEmailAsync(ChannelDeliveryRequest request)
        {
            try
            {
                var result = await _deliveryService.SendEmailAsync(request.RecipientId, request.Subject, request.Message);
                return ApplicationResponse<bool>.SuccessResponse(result, result ? "Email enviado" : "Error enviando email");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email directo");
                return ApplicationResponse<bool>.ErrorResponse("Error técnico enviando email", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> SendSmsAsync(ChannelDeliveryRequest request)
        {
            try
            {
                var result = await _deliveryService.SendSmsAsync(request.RecipientId, request.Message);
                return ApplicationResponse<bool>.SuccessResponse(result, result ? "SMS enviado" : "Error enviando SMS");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando SMS directo");
                return ApplicationResponse<bool>.ErrorResponse("Error técnico enviando SMS", "TechnicalError");
            }
        }

        public async Task<ApplicationResponse<bool>> SendWhatsAppAsync(ChannelDeliveryRequest request)
        {
            try
            {
                var result = await _deliveryService.SendWhatsAppAsync(request.RecipientId, request.Message);
                return ApplicationResponse<bool>.SuccessResponse(result, result ? "WhatsApp enviado" : "Error enviando WhatsApp");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando WhatsApp directo");
                return ApplicationResponse<bool>.ErrorResponse("Error técnico enviando WhatsApp", "TechnicalError");
            }
        }
    }
}
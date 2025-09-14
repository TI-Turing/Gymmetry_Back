using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO.UnifiedNotification;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IUnifiedNotificationService
    {
        // Envío unificado
        Task<ApplicationResponse<NotificationDeliveryResult>> SendUnifiedNotificationAsync(UnifiedNotificationRequest request);

        // Gestión de templates
        Task<ApplicationResponse<NotificationTemplate>> CreateTemplateAsync(NotificationTemplateRequest request);
        Task<ApplicationResponse<IEnumerable<NotificationTemplate>>> GetTemplatesAsync();
        Task<ApplicationResponse<IEnumerable<NotificationTemplate>>> GetTemplatesByTypeAsync(string notificationType);
        Task<ApplicationResponse<NotificationTemplate>> UpdateTemplateAsync(int templateId, NotificationTemplateRequest request);
        Task<ApplicationResponse<bool>> DeleteTemplateAsync(int templateId);

        // Gestión de preferencias
        Task<ApplicationResponse<IEnumerable<UserNotificationPreference>>> GetUserPreferencesAsync(Guid userId);
        Task<ApplicationResponse<UserNotificationPreference>> UpdateUserPreferencesAsync(UserPreferencesRequest request);

        // Envío por canal específico
        Task<ApplicationResponse<bool>> SendPushNotificationAsync(ChannelDeliveryRequest request);
        Task<ApplicationResponse<bool>> SendEmailAsync(ChannelDeliveryRequest request);
        Task<ApplicationResponse<bool>> SendSmsAsync(ChannelDeliveryRequest request);
        Task<ApplicationResponse<bool>> SendWhatsAppAsync(ChannelDeliveryRequest request);

        // Utilidades
        Task<ApplicationResponse<bool>> ValidateNotificationRulesAsync(string notificationType, Guid userId);
        Task<ApplicationResponse<string>> ProcessTemplateAsync(string templateKey, Dictionary<string, object> data);
    }

    public interface INotificationDeliveryService
    {
        Task<bool> SendPushNotificationAsync(string token, string title, string body, Dictionary<string, object>? metadata = null);
        Task<bool> SendEmailAsync(string email, string subject, string body);
        Task<bool> SendSmsAsync(string phone, string message);
        Task<bool> SendWhatsAppAsync(string phone, string message);
    }
}
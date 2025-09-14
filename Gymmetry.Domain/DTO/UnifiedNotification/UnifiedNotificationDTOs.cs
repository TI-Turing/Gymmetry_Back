using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.UnifiedNotification
{
    public class UnifiedNotificationRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string NotificationType { get; set; } = null!; // fitness, social, billing, security, moderation

        [Required]
        public string Priority { get; set; } = "normal"; // low, normal, high, critical

        [Required]
        public string TemplateKey { get; set; } = null!;

        public Dictionary<string, object> TemplateData { get; set; } = new();

        public string[]? ForcedChannels { get; set; } // Opcional: forzar canales específicos

        public bool SkipPreferences { get; set; } = false; // Para notificaciones críticas
    }

    public class NotificationDeliveryResult
    {
        public bool Success { get; set; }

        public Dictionary<string, bool> ChannelResults { get; set; } = new(); // push, email, sms, whatsapp

        public List<string> Errors { get; set; } = new();

        public Guid NotificationId { get; set; }
    }

    public class NotificationTemplateRequest
    {
        [Required]
        public string TemplateKey { get; set; } = null!;

        [Required]
        public string NotificationType { get; set; } = null!;

        [Required]
        public string Priority { get; set; } = null!;

        [Required]
        public string Subject { get; set; } = null!;

        [Required]
        public string BodyTemplate { get; set; } = null!;

        public string? EmailTemplate { get; set; }

        public string? SmsTemplate { get; set; }

        public string? WhatsAppTemplate { get; set; }

        public bool RequiresEmail { get; set; } = false;

        public bool RequiresSms { get; set; } = false;

        public string AllowedChannels { get; set; } = "push,app";
    }

    public class UserPreferencesRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string NotificationType { get; set; } = null!;

        public bool PushEnabled { get; set; } = true;

        public bool EmailEnabled { get; set; } = true;

        public bool SmsEnabled { get; set; } = false;

        public bool WhatsAppEnabled { get; set; } = false;

        public bool AppEnabled { get; set; } = true;
    }

    public class ChannelDeliveryRequest
    {
        [Required]
        public string RecipientId { get; set; } = null!; // Token para push, email para email, phone para SMS/WhatsApp

        [Required]
        public string Subject { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        public Dictionary<string, object>? Metadata { get; set; }
    }
}
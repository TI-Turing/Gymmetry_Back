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
}
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.UnifiedNotification
{
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
}
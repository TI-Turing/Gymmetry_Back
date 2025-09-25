using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.UnifiedNotification
{
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
}
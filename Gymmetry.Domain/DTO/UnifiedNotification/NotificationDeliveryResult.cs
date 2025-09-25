using System;
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.UnifiedNotification
{
    public class NotificationDeliveryResult
    {
        public bool Success { get; set; }

        public Dictionary<string, bool> ChannelResults { get; set; } = new(); // push, email, sms, whatsapp

        public List<string> Errors { get; set; } = new();

        public Guid NotificationId { get; set; }
    }
}
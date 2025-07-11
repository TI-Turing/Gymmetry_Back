using System;

namespace FitGymApp.Domain.DTO.NotificationOption.Request
{
    public class UpdateNotificationOptionRequest
    {
        public Guid Id { get; set; }
        public string Mail { get; set; } = null!;
        public string Push { get; set; } = null!;
        public string App { get; set; } = null!;
        public string WhatsaApp { get; set; } = null!;
        public string Sms { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
        public Guid NotificationOptionNotificationNotificationOptionId { get; set; }
    }
}

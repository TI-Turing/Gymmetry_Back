using System;

namespace Gymmetry.Domain.Models;

public partial class UserNotificationPreference
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string NotificationType { get; set; } = null!;

    public bool PushEnabled { get; set; } = true;

    public bool EmailEnabled { get; set; } = true;

    public bool SmsEnabled { get; set; } = false;

    public bool WhatsAppEnabled { get; set; } = false;

    public bool AppEnabled { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }

    public virtual User User { get; set; } = null!;
}
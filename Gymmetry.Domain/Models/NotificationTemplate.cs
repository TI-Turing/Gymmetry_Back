using System;

namespace Gymmetry.Domain.Models;

public partial class NotificationTemplate
{
    public int Id { get; set; }

    public string TemplateKey { get; set; } = null!;

    public string NotificationType { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public string BodyTemplate { get; set; } = null!;

    public string? EmailTemplate { get; set; }

    public string? SmsTemplate { get; set; }

    public string? WhatsAppTemplate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool RequiresEmail { get; set; } = false;

    public bool RequiresSms { get; set; } = false;

    public string AllowedChannels { get; set; } = "push,app";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedDate { get; set; }
}
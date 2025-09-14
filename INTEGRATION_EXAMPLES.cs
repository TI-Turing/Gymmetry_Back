// Ejemplo de integración con sistemas existentes - no ejecutar, solo documentación

// En UserBlockService, después de crear un bloqueo:
await _unifiedNotificationService.SendUnifiedNotificationAsync(new UnifiedNotificationRequest
{
    UserId = blockedUserId,
    NotificationType = "moderation",
    Priority = "high",
    TemplateKey = "account_warning",
    TemplateData = new Dictionary<string, object>
    {
        { "reason", "Comportamiento inapropiado" },
        { "duration", "7 días" }
    }
});

// En PaymentService, cuando hay un error de pago:
await _unifiedNotificationService.SendUnifiedNotificationAsync(new UnifiedNotificationRequest
{
    UserId = userId,
    NotificationType = "billing",
    Priority = "critical",
    TemplateKey = "payment_failed",
    TemplateData = new Dictionary<string, object>
    {
        { "amount", failedAmount.ToString("C") }
    }
});

// En ContentModerationService, cuando se reporta contenido:
await _unifiedNotificationService.SendUnifiedNotificationAsync(new UnifiedNotificationRequest
{
    UserId = contentOwnerId,
    NotificationType = "moderation",
    Priority = "high",
    TemplateKey = "content_reported",
    TemplateData = new Dictionary<string, object>
    {
        { "contentType", "publicación" },
        { "reason", "contenido ofensivo" }
    }
});

// En FeedService, cuando alguien comenta:
await _unifiedNotificationService.SendUnifiedNotificationAsync(new UnifiedNotificationRequest
{
    UserId = postOwnerId,
    NotificationType = "social",
    Priority = "low",
    TemplateKey = "new_comment",
    TemplateData = new Dictionary<string, object>
    {
        { "username", commenterName },
        { "comment", commentText.Substring(0, Math.Min(50, commentText.Length)) + "..." }
    }
});

// Para recordatorios de rutina (Timer Function):
await _unifiedNotificationService.SendUnifiedNotificationAsync(new UnifiedNotificationRequest
{
    UserId = userId,
    NotificationType = "fitness",
    Priority = "normal", 
    TemplateKey = "workout_reminder",
    TemplateData = new Dictionary<string, object>()
});

// Para logros de racha:
await _unifiedNotificationService.SendUnifiedNotificationAsync(new UnifiedNotificationRequest
{
    UserId = userId,
    NotificationType = "fitness",
    Priority = "normal",
    TemplateKey = "streak_achievement",
    TemplateData = new Dictionary<string, object>
    {
        { "days", streakDays.ToString() }
    }
});
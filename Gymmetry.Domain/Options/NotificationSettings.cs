namespace Gymmetry.Domain.Options
{
    public class MailGunSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public class TwilioSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromPhone { get; set; } = string.Empty; // Para SMS
        public string WhatsAppNumber { get; set; } = string.Empty; // Para WhatsApp
    }

    public class ExpoPushSettings
    {
        public string AccessToken { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = "https://exp.host/--/api/v2/push/send";
    }

    public class NotificationChannelRules
    {
        public static readonly Dictionary<string, string[]> DefaultChannels = new()
        {
            { "fitness", new[] { "push", "app" } },
            { "social", new[] { "push", "app" } },
            { "billing", new[] { "push", "app", "email" } },
            { "security", new[] { "push", "app", "email", "sms" } },
            { "moderation", new[] { "push", "app", "email" } }
        };

        public static readonly Dictionary<string, string[]> CriticalChannels = new()
        {
            { "critical", new[] { "push", "app", "email", "sms" } }
        };
    }
}
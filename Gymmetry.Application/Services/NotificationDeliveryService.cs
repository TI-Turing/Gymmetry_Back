using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Gymmetry.Application.Services.Interfaces;
using Gymmetry.Domain.Options;

namespace Gymmetry.Application.Services
{
    public class NotificationDeliveryService : INotificationDeliveryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationDeliveryService> _logger;
        private readonly MailGunSettings _mailGunSettings;
        private readonly TwilioSettings _twilioSettings;
        private readonly ExpoPushSettings _expoPushSettings;

        public NotificationDeliveryService(
            HttpClient httpClient,
            ILogger<NotificationDeliveryService> logger,
            IOptions<MailGunSettings> mailGunSettings,
            IOptions<TwilioSettings> twilioSettings,
            IOptions<ExpoPushSettings> expoPushSettings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _mailGunSettings = mailGunSettings.Value;
            _twilioSettings = twilioSettings.Value;
            _expoPushSettings = expoPushSettings.Value;
        }

        public async Task<bool> SendPushNotificationAsync(string token, string title, string body, Dictionary<string, object>? metadata = null)
        {
            try
            {
                var payload = new
                {
                    to = token,
                    title,
                    body,
                    data = metadata ?? new Dictionary<string, object>()
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(_expoPushSettings.AccessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _expoPushSettings.AccessToken);
                }

                var response = await _httpClient.PostAsync(_expoPushSettings.ApiUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Push notification sent successfully to token: {Token}", MaskToken(token));
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send push notification. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to token: {Token}", MaskToken(token));
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                var formData = new List<KeyValuePair<string, string>>
                {
                    new("from", $"{_mailGunSettings.FromName} <{_mailGunSettings.FromEmail}>"),
                    new("to", email),
                    new("subject", subject),
                    new("html", body)
                };

                var formContent = new FormUrlEncodedContent(formData);

                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_mailGunSettings.ApiKey}"));
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                var response = await _httpClient.PostAsync(
                    $"https://api.mailgun.net/v3/{_mailGunSettings.Domain}/messages", 
                    formContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to: {Email}", MaskEmail(email));
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send email. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to: {Email}", MaskEmail(email));
                return false;
            }
        }

        public async Task<bool> SendSmsAsync(string phone, string message)
        {
            try
            {
                var formData = new List<KeyValuePair<string, string>>
                {
                    new("From", _twilioSettings.FromPhone),
                    new("To", phone),
                    new("Body", message)
                };

                var formContent = new FormUrlEncodedContent(formData);

                var authToken = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{_twilioSettings.AccountSid}:{_twilioSettings.AuthToken}"));
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                var response = await _httpClient.PostAsync(
                    $"https://api.twilio.com/2010-04-01/Accounts/{_twilioSettings.AccountSid}/Messages.json", 
                    formContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS sent successfully to: {Phone}", MaskPhone(phone));
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send SMS. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to: {Phone}", MaskPhone(phone));
                return false;
            }
        }

        public async Task<bool> SendWhatsAppAsync(string phone, string message)
        {
            try
            {
                var formData = new List<KeyValuePair<string, string>>
                {
                    new("From", $"whatsapp:{_twilioSettings.WhatsAppNumber}"),
                    new("To", $"whatsapp:{phone}"),
                    new("Body", message)
                };

                var formContent = new FormUrlEncodedContent(formData);

                var authToken = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{_twilioSettings.AccountSid}:{_twilioSettings.AuthToken}"));
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                var response = await _httpClient.PostAsync(
                    $"https://api.twilio.com/2010-04-01/Accounts/{_twilioSettings.AccountSid}/Messages.json", 
                    formContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("WhatsApp message sent successfully to: {Phone}", MaskPhone(phone));
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send WhatsApp message. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp message to: {Phone}", MaskPhone(phone));
                return false;
            }
        }

        private static string MaskToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length <= 8) return "***";
            return $"{token[..4]}***{token[^4..]}";
        }

        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return "***";
            var parts = email.Split('@');
            if (parts.Length != 2) return "***";
            var localPart = parts[0];
            var domainPart = parts[1];
            return $"{(localPart.Length > 2 ? localPart[..2] : localPart)}***@{domainPart}";
        }

        private static string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length <= 4) return "***";
            return $"***{phone[^4..]}";
        }
    }
}
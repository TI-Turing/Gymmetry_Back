using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.UnifiedNotification
{
    public class ChannelDeliveryRequest
    {
        [Required]
        public string RecipientId { get; set; } = null!; // Token para push, email para email, phone para SMS/WhatsApp

        [Required]
        public string Subject { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        public Dictionary<string, object>? Metadata { get; set; }
    }
}
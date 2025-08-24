using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymmetry.Domain.Models
{
    [Table("PaymentIntents")]
    public class PaymentIntent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PreferenceId { get; set; } = default!; // Mercado Pago preference id
        public string? ExternalPaymentId { get; set; } // MP payment id
        public Guid? UserId { get; set; }
        public Guid? GymId { get; set; }
        public Guid? PlanTypeId { get; set; }
        public Guid? GymPlanSelectedTypeId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "COP";
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? RawPreferenceJson { get; set; }
        public string? RawPaymentJson { get; set; }
        public string? Hash { get; set; }
        public Guid? CreatedPlanId { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastStatusCheckAt { get; set; }
        public string? PaymentMethod { get; set; }
        public string? BankCode { get; set; }
    }
}

using System;
using Gymmetry.Domain.Models;

namespace Gymmetry.Domain.DTO.Payments.Responses
{
    public class PaymentStatusResponse
    {
        public Guid PaymentId { get; set; }
        public string PreferenceId { get; set; } = default!;
        public PaymentStatus Status { get; set; }
        public bool PlanCreated { get; set; }
        public string Type { get; set; } = default!; // user|gym
        public Guid? CreatedPlanId { get; set; }
        public string? PaymentMethod { get; set; }
        public string? BankCode { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
    }
}

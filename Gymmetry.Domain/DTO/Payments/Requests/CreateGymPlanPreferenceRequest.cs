using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.Payments.Requests
{
    public class CreateGymPlanPreferenceRequest
    {
        [Required]
        public Guid GymPlanSelectedTypeId { get; set; }
        [Required]
        public Guid GymId { get; set; }
        [Required]
        public Guid UserId { get; set; } // Owner del gym que inicia el pago
        public string? SuccessUrl { get; set; }
        public string? FailureUrl { get; set; }
        public string? PendingUrl { get; set; }
        public string? PaymentMethod { get; set; } = "CARD"; // CARD o PSE
        public string? BankCode { get; set; }
        public string? BuyerEmail { get; set; }
    }
}

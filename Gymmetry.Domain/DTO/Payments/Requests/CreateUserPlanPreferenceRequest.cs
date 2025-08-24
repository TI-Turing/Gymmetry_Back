using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.Payments.Requests
{
    public class CreateUserPlanPreferenceRequest
    {
        [Required]
        public Guid PlanTypeId { get; set; }
        [Required]
        public Guid UserId { get; set; }
        public string? SuccessUrl { get; set; }
        public string? FailureUrl { get; set; }
        public string? PendingUrl { get; set; }
        // Nuevo: m�todo de pago (CARD, PSE, etc.)
        public string? PaymentMethod { get; set; } = "CARD";
        // C�digo banco para PSE (opcional)
        public string? BankCode { get; set; }
        // Email comprador (si se quiere sobreescribir)
        public string? BuyerEmail { get; set; }
    }
}

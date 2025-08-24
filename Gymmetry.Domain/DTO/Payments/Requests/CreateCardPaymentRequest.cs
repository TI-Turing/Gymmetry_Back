using System;
using System.ComponentModel.DataAnnotations;

namespace Gymmetry.Domain.DTO.Payments.Requests
{
    public class CreateCardPaymentRequest
    {
        [Required]
        public Guid UserId { get; set; }
        public Guid? PlanTypeId { get; set; } // user flow
        public Guid? GymId { get; set; } // gym flow
        public Guid? GymPlanSelectedTypeId { get; set; } // gym flow
        [Required]
        public string CardToken { get; set; } = default!; // MP Bricks token
        public int Installments { get; set; } = 1;
        public string? BuyerEmail { get; set; }
    }
}

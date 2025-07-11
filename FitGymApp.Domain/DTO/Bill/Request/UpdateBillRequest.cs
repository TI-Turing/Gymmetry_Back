using System;

namespace FitGymApp.Domain.DTO.Bill.Request
{
    public class UpdateBillRequest
    {
        public Guid Id { get; set; }
        public string Ammount { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid UserTypeId { get; set; }
        public Guid UserId { get; set; }
        public Guid UserSellerId { get; set; }
        public Guid GymId { get; set; }
    }
}

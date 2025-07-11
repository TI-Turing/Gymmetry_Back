using System;

namespace FitGymApp.Domain.DTO.Machine.Request
{
    public class UpdateMachineRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Observations { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid MachineCategoryId { get; set; }
        public Guid BrandId { get; set; }
    }
}

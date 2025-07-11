using System;

namespace FitGymApp.Domain.DTO.Machine.Request
{
    public class AddMachineRequest
    {
        public string Name { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Observations { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid MachineCategoryId { get; set; }
        public Guid BrandId { get; set; }
    }
}

using System;

namespace FitGymApp.Domain.DTO.Brand.Request
{
    public class AddBrandRequest
    {
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

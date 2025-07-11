using System;

namespace FitGymApp.Domain.DTO.UninstallOption.Request
{
    public class AddUninstallOptionRequest
    {
        public string Name { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

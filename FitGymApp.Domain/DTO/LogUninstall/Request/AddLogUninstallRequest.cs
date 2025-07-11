using System;

namespace FitGymApp.Domain.DTO.LogUninstall.Request
{
    public class AddLogUninstallRequest
    {
        public string? Comments { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid UserId { get; set; }
        public Guid UnnistallOptionsId { get; set; }
    }
}

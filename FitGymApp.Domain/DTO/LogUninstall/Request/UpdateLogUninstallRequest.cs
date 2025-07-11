using System;

namespace FitGymApp.Domain.DTO.LogUninstall.Request
{
    public class UpdateLogUninstallRequest
    {
        public Guid Id { get; set; }
        public string? Comments { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
        public Guid UnnistallOptionsId { get; set; }
    }
}

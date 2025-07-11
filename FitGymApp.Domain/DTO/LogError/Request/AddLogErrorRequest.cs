using System;

namespace FitGymApp.Domain.DTO.LogError.Request
{
    public class AddLogErrorRequest
    {
        public string Error { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid UserId { get; set; }
        public Guid SubModuleId { get; set; }
    }
}

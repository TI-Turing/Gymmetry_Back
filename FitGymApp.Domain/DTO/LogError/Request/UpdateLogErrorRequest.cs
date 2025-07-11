using System;

namespace FitGymApp.Domain.DTO.LogError.Request
{
    public class UpdateLogErrorRequest
    {
        public Guid Id { get; set; }
        public string Error { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
        public Guid SubModuleId { get; set; }
    }
}

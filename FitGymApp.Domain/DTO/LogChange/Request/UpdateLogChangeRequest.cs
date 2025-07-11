using System;

namespace FitGymApp.Domain.DTO.LogChange.Request
{
    public class UpdateLogChangeRequest
    {
        public Guid Id { get; set; }
        public string Table { get; set; } = null!;
        public string PastObject { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
    }
}

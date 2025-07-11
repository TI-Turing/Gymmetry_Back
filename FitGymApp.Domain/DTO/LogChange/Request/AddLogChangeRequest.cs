using System;

namespace FitGymApp.Domain.DTO.LogChange.Request
{
    public class AddLogChangeRequest
    {
        public string Table { get; set; } = null!;
        public string PastObject { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid UserId { get; set; }
    }
}

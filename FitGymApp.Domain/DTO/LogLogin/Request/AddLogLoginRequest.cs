using System;

namespace FitGymApp.Domain.DTO.LogLogin.Request
{
    public class AddLogLoginRequest
    {
        public bool IsSuccess { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid UserId { get; set; }
    }
}

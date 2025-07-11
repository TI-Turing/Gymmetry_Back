using System;

namespace FitGymApp.Domain.DTO.LogLogin.Request
{
    public class UpdateLogLoginRequest
    {
        public Guid Id { get; set; }
        public bool IsSuccess { get; set; }
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
    }
}

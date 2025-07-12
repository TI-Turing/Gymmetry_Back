using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.LogLogin.Request
{
    public class UpdateLogLoginRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public bool IsSuccess { get; set; }
        public Guid UserId { get; set; }
    }
}

using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.UserType.Request
{
    public class UpdateUserTypeRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}

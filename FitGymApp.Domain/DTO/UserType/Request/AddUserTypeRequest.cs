using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.UserType.Request
{
    public class AddUserTypeRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
    }
}

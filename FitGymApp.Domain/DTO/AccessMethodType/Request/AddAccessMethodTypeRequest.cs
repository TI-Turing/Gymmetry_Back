using System;

namespace FitGymApp.Domain.DTO.AccessMethodType.Request
{
    public class AddAccessMethodTypeRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
    }
}

using System;

namespace FitGymApp.Domain.DTO.AccessMethodType.Request
{
    public class UpdateAccessMethodTypeRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}

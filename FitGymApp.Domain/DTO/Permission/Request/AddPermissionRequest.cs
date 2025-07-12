using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.Permission.Request
{
    public class AddPermissionRequest : ApiRequest
    {
        public string See { get; set; } = null!;
        public string Create { get; set; } = null!;
        public string Read { get; set; } = null!;
        public string Update { get; set; } = null!;
        public string Delete { get; set; } = null!;
        public Guid UserTypeId { get; set; }
        public Guid UserId { get; set; }
    }
}

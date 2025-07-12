using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.LogChange.Request
{
    public class AddLogChangeRequest : ApiRequest
    {
        public string Table { get; set; } = null!;
        public string PastObject { get; set; } = null!;
        public Guid UserId { get; set; }
    }
}

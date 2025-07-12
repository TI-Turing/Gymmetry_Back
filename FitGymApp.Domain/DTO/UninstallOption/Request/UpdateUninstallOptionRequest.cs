using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.UninstallOption.Request
{
    public class UpdateUninstallOptionRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}

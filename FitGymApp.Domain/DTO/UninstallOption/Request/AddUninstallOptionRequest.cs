using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.UninstallOption.Request
{
    public class AddUninstallOptionRequest : ApiRequest
    {
        public string Name { get; set; } = null!;
    }
}

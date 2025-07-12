using System;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Domain.DTO.LogUninstall.Request
{
    public class UpdateLogUninstallRequest : ApiRequest
    {
        public Guid Id { get; set; }
        public string? Comments { get; set; }
        public Guid UserId { get; set; }
        public Guid UnnistallOptionsId { get; set; }
    }
}

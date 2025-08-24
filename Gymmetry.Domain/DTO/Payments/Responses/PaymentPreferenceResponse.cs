using System;

namespace Gymmetry.Domain.DTO.Payments.Responses
{
    public class PaymentPreferenceResponse
    {
        public string Id { get; set; } = default!; // PreferenceId
        public string PreferenceId => Id; // alias
        public string InitPoint { get; set; } = default!;
        public string? SandboxInitPoint { get; set; }
        public string Status { get; set; } = "pending";
    }
}

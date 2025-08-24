using Gymmetry.Domain.Models;
namespace Gymmetry.Domain.DTO.Payments.Responses
{
    public class PreferenceCreationResult
    {
        public string PreferenceId { get; set; } = default!;
        public string InitPoint { get; set; } = default!;
        public string? SandboxInitPoint { get; set; }
        public string RawJson { get; set; } = default!;
    }
    public class PaymentDetailsResult
    {
        public PaymentStatus Status { get; set; }
        public string RawJson { get; set; } = string.Empty;
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? PreferenceId { get; set; }
        public string? ExternalPaymentId { get; set; }
    }
}

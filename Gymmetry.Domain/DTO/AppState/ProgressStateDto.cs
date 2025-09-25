namespace Gymmetry.Domain.DTO.AppState
{
    public class ProgressStateDto
    {
        public ProgressSummaryDto Summary { get; set; } = new();
        public string DefaultPeriod { get; set; } = "3months";
    }
}
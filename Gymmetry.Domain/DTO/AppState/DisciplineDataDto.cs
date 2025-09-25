namespace Gymmetry.Domain.DTO.AppState
{
    public class DisciplineDataDto
    {
        public decimal CompletionPercentage { get; set; }
        public int CompletedDays { get; set; }
        public int TotalExpectedDays { get; set; }
        public int CurrentStreak { get; set; }
        public decimal ConsistencyIndex { get; set; }
        public string PeriodDescription { get; set; } = "Últimas 4 semanas";
    }
}
using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.Progress
{
    public class HistoryPoint
    {
        public string PeriodFrom { get; set; } = string.Empty;
        public string PeriodTo { get; set; } = string.Empty;
        public decimal AdherencePct { get; set; }
        public int Sessions { get; set; }
        public int CompletedDays { get; set; }
        public int TargetDays { get; set; }
        public int TotalMinutes { get; set; }
        public decimal AvgCompletion { get; set; }
        public decimal BalanceIndex { get; set; }
    }

    public class MultiProgressHistoryResponse
    {
        public List<ProgressSummaryResponse> Periods { get; set; } = new();
        public List<HistoryPoint> History { get; set; } = new();
    }
}

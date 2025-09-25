using System;

namespace Gymmetry.Domain.DTO.AppState
{
    public class AppStateOverviewDto
    {
        public HomeStateDto Home { get; set; } = new();
        public GymStateDto Gym { get; set; } = new();
        public ProgressStateDto Progress { get; set; } = new();
        public FeedStateDto Feed { get; set; } = new();
        public ProfileStateDto Profile { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }
}
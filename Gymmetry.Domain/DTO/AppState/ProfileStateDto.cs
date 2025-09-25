namespace Gymmetry.Domain.DTO.AppState
{
    public class ProfileStateDto
    {
        public Gymmetry.Domain.Models.User? UserProfile { get; set; }
        public Gymmetry.Domain.Models.PhysicalAssessment? LatestAssessment { get; set; }
        public ProfileStatsDto Stats { get; set; } = new();
    }
}
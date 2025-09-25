using System.Collections.Generic;

namespace Gymmetry.Domain.DTO.AppState
{
    public class GymStateDto
    {
        public Gymmetry.Domain.Models.Gym? GymData { get; set; }
        public bool IsConnectedToGym { get; set; }
        public string? GymId { get; set; }
        public List<Gymmetry.Domain.Models.Branch> AvailableBranches { get; set; } = new();
    }
}
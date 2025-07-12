using System;

namespace FitGymApp.Domain.DTO.Branch.Request
{
    public class AddBranchRequest : ApiRequest
    {
        public string Address { get; set; } = null!;
        public Guid CityId { get; set; }
        public Guid RegionId { get; set; }
        public Guid GymId { get; set; }
        public Guid BranchDailyBranchId { get; set; }
        public Guid AccessMethodId { get; set; }
    }
}

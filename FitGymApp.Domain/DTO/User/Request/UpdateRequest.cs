using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitGymApp.Domain.DTO.User.Request
{
    public class UpdateRequest
    {
        public Guid Id { get; set; }
        public Guid IdEps { get; set; }
        public string Name { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public Guid IdGender { get; set; }
        public DateTime BirthDate { get; set; }
        public Guid DocumentTypeId { get; set; }
        public string? DocumentType { get; set; }
        public string? Phone { get; set; }
        public Guid CountryId { get; set; }
        public string? Address { get; set; }
        public Guid CityId { get; set; }
        public Guid? RegionId { get; set; }
        public string? Rh { get; set; }
        public string? EmergencyName { get; set; }
        public string? EmergencyPhone { get; set; }
        public string? PhysicalExceptions { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid UserTypeId { get; set; }
    }
}

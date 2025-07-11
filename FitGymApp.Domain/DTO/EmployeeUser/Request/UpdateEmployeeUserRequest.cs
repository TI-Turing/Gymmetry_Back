using System;

namespace FitGymApp.Domain.DTO.EmployeeUser.Request
{
    public class UpdateEmployeeUserRequest
    {
        public Guid Id { get; set; }
        public string Arl { get; set; } = null!;
        public string PensionFund { get; set; } = null!;
        public DateTime StartContract { get; set; }
        public DateTime? EndContract { get; set; }
        public string BankId { get; set; } = null!;
        public string AccountType { get; set; } = null!;
        public string AccountNumber { get; set; } = null!;
        public string Salary { get; set; } = null!;
        public string? Ip { get; set; }
        public bool IsActive { get; set; }
        public Guid EmployeeTypeId { get; set; }
    }
}

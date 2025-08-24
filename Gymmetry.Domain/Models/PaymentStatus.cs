using System;

namespace Gymmetry.Domain.Models
{
    public enum PaymentStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3,
        Expired = 4
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ApplicationResponse<PaymentIntent>> GetPaymentByIdAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<PaymentIntent>>> GetAllPaymentsAsync();
        Task<ApplicationResponse<IEnumerable<PaymentIntent>>> FindPaymentsByFieldsAsync(Dictionary<string, object> filters);
    }
}

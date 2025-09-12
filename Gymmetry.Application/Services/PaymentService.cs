using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.Models;
using Gymmetry.Repository.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Gymmetry.Application.Services.Interfaces;

namespace Gymmetry.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentIntentRepository _repo;
        private readonly ILogger<PaymentService> _logger;
        public PaymentService(IPaymentIntentRepository repo, ILogger<PaymentService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<ApplicationResponse<PaymentIntent>> GetPaymentByIdAsync(Guid id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null || entity.DeletedAt != null || entity.IsActive == false)
            {
                return ApplicationResponse<PaymentIntent>.ErrorResponse("Pago no encontrado.", "NotFound");
            }
            return ApplicationResponse<PaymentIntent>.SuccessResponse(entity);
        }

        public async Task<ApplicationResponse<IEnumerable<PaymentIntent>>> GetAllPaymentsAsync()
        {
            var list = await _repo.GetAllAsync();
            return ApplicationResponse<IEnumerable<PaymentIntent>>.SuccessResponse(list);
        }

        public async Task<ApplicationResponse<IEnumerable<PaymentIntent>>> FindPaymentsByFieldsAsync(Dictionary<string, object> filters)
        {
            var list = await _repo.FindByFieldsAsync(filters);
            return ApplicationResponse<IEnumerable<PaymentIntent>>.SuccessResponse(list);
        }
    }
}

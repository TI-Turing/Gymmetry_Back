using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Bill.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IBillService
    {
        Task<ApplicationResponse<Bill>> CreateBillAsync(AddBillRequest request);
        Task<ApplicationResponse<Bill>> GetBillByIdAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<Bill>>> GetAllBillsAsync();
        Task<ApplicationResponse<bool>> UpdateBillAsync(UpdateBillRequest request);
        Task<ApplicationResponse<bool>> DeleteBillAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<Bill>>> FindBillsByFieldsAsync(Dictionary<string, object> filters);
    }
}

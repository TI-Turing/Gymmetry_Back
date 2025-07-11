using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Bill.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IBillService
    {
        ApplicationResponse<Bill> CreateBill(AddBillRequest request);
        ApplicationResponse<Bill> GetBillById(Guid id);
        ApplicationResponse<IEnumerable<Bill>> GetAllBills();
        ApplicationResponse<bool> UpdateBill(UpdateBillRequest request);
        ApplicationResponse<bool> DeleteBill(Guid id);
        ApplicationResponse<IEnumerable<Bill>> FindBillsByFields(Dictionary<string, object> filters);
    }
}

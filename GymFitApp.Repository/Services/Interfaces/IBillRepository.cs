using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IBillRepository
    {
        Bill CreateBill(Bill entity);
        Bill GetBillById(Guid id);
        IEnumerable<Bill> GetAllBills();
        bool UpdateBill(Bill entity);
        bool DeleteBill(Guid id);
        IEnumerable<Bill> FindBillsByFields(Dictionary<string, object> filters);
    }
}

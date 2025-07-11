using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IBranchRepository
    {
        Branch CreateBranch(Branch branch);
        Branch GetBranchById(Guid id);
        IEnumerable<Branch> GetAllBranches();
        bool UpdateBranch(Branch branch);
        bool DeleteBranch(Guid id);
        IEnumerable<Branch> FindBranchesByFields(Dictionary<string, object> filters);
    }
}

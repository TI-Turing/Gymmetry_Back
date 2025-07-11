using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Branch.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IBranchService
    {
        ApplicationResponse<Branch> CreateBranch(AddBranchRequest request);
        ApplicationResponse<Branch> GetBranchById(Guid id);
        ApplicationResponse<IEnumerable<Branch>> GetAllBranches();
        ApplicationResponse<bool> UpdateBranch(UpdateBranchRequest request);
        ApplicationResponse<bool> DeleteBranch(Guid id);
        ApplicationResponse<IEnumerable<Branch>> FindBranchesByFields(Dictionary<string, object> filters);
    }
}

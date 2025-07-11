using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;
using FitGymApp.Domain.DTO.Machine.Request;
using FitGymApp.Domain.DTO;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IMachineService
    {
        ApplicationResponse<Machine> CreateMachine(AddMachineRequest request);
        ApplicationResponse<Machine> GetMachineById(Guid id);
        ApplicationResponse<IEnumerable<Machine>> GetAllMachines();
        ApplicationResponse<bool> UpdateMachine(UpdateMachineRequest request);
        ApplicationResponse<bool> DeleteMachine(Guid id);
        ApplicationResponse<IEnumerable<Machine>> FindMachinesByFields(Dictionary<string, object> filters);
    }
}

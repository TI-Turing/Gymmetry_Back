using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IMachineRepository
    {
        Machine CreateMachine(Machine machine);
        Machine GetMachineById(Guid id);
        IEnumerable<Machine> GetAllMachines();
        bool UpdateMachine(Machine machine);
        bool DeleteMachine(Guid id);
        IEnumerable<Machine> FindMachinesByFields(Dictionary<string, object> filters);
    }
}

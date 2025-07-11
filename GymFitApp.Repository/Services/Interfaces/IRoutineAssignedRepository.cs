using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IRoutineAssignedRepository
    {
        RoutineAssigned CreateRoutineAssigned(RoutineAssigned entity);
        RoutineAssigned GetRoutineAssignedById(Guid id);
        IEnumerable<RoutineAssigned> GetAllRoutineAssigneds();
        bool UpdateRoutineAssigned(RoutineAssigned entity);
        bool DeleteRoutineAssigned(Guid id);
        IEnumerable<RoutineAssigned> FindRoutineAssignedsByFields(Dictionary<string, object> filters);
    }
}

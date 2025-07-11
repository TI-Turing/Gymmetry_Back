using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IUserTypeRepository
    {
        UserType CreateUserType(UserType entity);
        UserType GetUserTypeById(Guid id);
        IEnumerable<UserType> GetAllUserTypes();
        bool UpdateUserType(UserType entity);
        bool DeleteUserType(Guid id);
        IEnumerable<UserType> FindUserTypesByFields(Dictionary<string, object> filters);
    }
}

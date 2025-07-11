using System;
using System.Collections.Generic;
using FitGymApp.Domain.Models;

namespace GymFitApp.Repository.Services.Interfaces
{
    public interface IUserRepository
    {
        User CreateUser(User user);
        User GetUserById(Guid id);
        IEnumerable<User> GetAllUsers();
        bool UpdateUser(User user);
        bool DeleteUser(Guid id);
        IEnumerable<User> FindUsersByFields(Dictionary<string, object> filters);
    }
}

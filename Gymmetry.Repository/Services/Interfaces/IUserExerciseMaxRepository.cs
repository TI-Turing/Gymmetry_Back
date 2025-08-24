using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IUserExerciseMaxRepository
    {
        Task<UserExerciseMax> CreateAsync(UserExerciseMax entity);
        Task<UserExerciseMax?> GetByIdAsync(Guid id);
        Task<IEnumerable<UserExerciseMax>> GetAllAsync();
        Task<bool> UpdateAsync(UserExerciseMax entity);
        Task<bool> DeleteAsync(Guid id);
        Task<IEnumerable<UserExerciseMax>> FindByFieldsAsync(Dictionary<string, object> filters);
    }
}

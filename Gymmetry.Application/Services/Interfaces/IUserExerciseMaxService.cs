using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.DTO;
using Gymmetry.Domain.DTO.UserExerciseMax.Request;
using Gymmetry.Domain.Models;

namespace Gymmetry.Application.Services.Interfaces
{
    public interface IUserExerciseMaxService
    {
        Task<ApplicationResponse<UserExerciseMax>> CreateAsync(AddUserExerciseMaxRequest request);
        Task<ApplicationResponse<UserExerciseMax>> GetByIdAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<UserExerciseMax>>> GetAllAsync();
        Task<ApplicationResponse<bool>> UpdateAsync(UpdateUserExerciseMaxRequest request, Guid? userId, string ip = "", string invocationId = "");
        Task<ApplicationResponse<bool>> DeleteAsync(Guid id);
        Task<ApplicationResponse<IEnumerable<UserExerciseMax>>> FindByFieldsAsync(Dictionary<string, object> filters);
    }
}

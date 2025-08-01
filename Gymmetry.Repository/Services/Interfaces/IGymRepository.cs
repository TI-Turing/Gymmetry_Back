using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gymmetry.Domain.Models;

namespace Gymmetry.Repository.Services.Interfaces
{
    public interface IGymRepository
    {
        Task<Gym> CreateGymAsync(Gym entity);
        Task<Gym?> GetGymByIdAsync(Guid id);
        Task<IEnumerable<Gym>> GetAllGymsAsync();
        Task<bool> UpdateGymAsync(Gym entity);
        Task<bool> DeleteGymAsync(Guid id);
        Task<IEnumerable<Gym>> FindGymsByFieldsAsync(Dictionary<string, object> filters);
        Task<string> UploadGymLogoAsync(Guid gymId, byte[] image, string? fileName, string? contentType);
        Task<string?> GetLogoFromBlobStorageAsync(Guid gymId);
        Task SaveChangesAsync();
        Task<IEnumerable<Gym>> FindGymsByNameAsync(string name);
    }
}

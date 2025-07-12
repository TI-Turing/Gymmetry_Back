using System.Threading.Tasks;
using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface ILogErrorRepository
    {
        Task<bool> AddAsync(LogError log);
    }
    public interface ILogLoginRepository
    {
        Task<bool> AddAsync(LogLogin log);
    }
    public interface ILogChangeRepository
    {
        Task<bool> AddAsync(LogChange log);
    }
}

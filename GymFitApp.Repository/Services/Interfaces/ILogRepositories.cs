using FitGymApp.Domain.Models;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface ILogErrorRepository
    {
        bool Add(LogError log);
    }
    public interface ILogLoginRepository
    {
        bool Add(LogLogin log);
    }
    public interface ILogChangeRepository
    {
        bool Add(LogChange log);
    }
}

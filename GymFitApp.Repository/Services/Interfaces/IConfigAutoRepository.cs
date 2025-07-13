using System.Threading.Tasks;

namespace FitGymApp.Repository.Services.Interfaces
{
    public interface IConfigAutoRepository
    {
        Task UpdateUsdPricesAsync(decimal usdToCopRate);
    }
}

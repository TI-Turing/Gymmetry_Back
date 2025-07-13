using System.Threading.Tasks;

namespace FitGymApp.Application.Services.Interfaces
{
    public interface IConfigAutoService
    {
        Task UpdateUsdPricesFromExchangeAsync();
    }
}

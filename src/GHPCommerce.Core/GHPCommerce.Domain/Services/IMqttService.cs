using System.Threading.Tasks;

namespace GHPCommerce.Domain.Services
{
    public interface IMqttService
    {
        Task StartAsync();
    }
}
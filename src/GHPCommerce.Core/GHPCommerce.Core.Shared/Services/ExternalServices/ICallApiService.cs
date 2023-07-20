using System.Threading.Tasks;
using RestSharp;

namespace GHPCommerce.Core.Shared.Services.ExternalServices
{
    public interface ICallApiService
    {
        Task<IRestResponse<T>> ExecuteRestRequestAsync<T>(string endpoint, object? body, Method method,
            string? token = null);
    }
}
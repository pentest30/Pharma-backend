using System.Threading.Tasks;
using RestSharp;

namespace GHPCommerce.Core.Shared.Services.ExternalServices
{
    public class CallApiService : ICallApiService
    {
        public async Task<IRestResponse<T>> ExecuteRestRequestAsync<T>(string endpoint, object? body, Method method, string? token = null)
        {
            var url = $"{endpoint}";
            RestClient restClient = new RestClient(url);
            RestRequest request = new RestRequest();
            request.AddHeader("Accept", "application/json; charset=utf-8 ");
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", $"Bearer {token}");
            }

            if (method != Method.GET )
            {
                request.RequestFormat = DataFormat.Json;
                request.Method = method;
                if (body != null)
                {
                    request.AddJsonBody(body);
                }
            }
            var rsp = await restClient.ExecuteAsync<T>(request);
            return rsp;
        }
    }
}
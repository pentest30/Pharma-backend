using System;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Ecommerce.ConfigurationOptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using RestSharp;

namespace GHPCommerce.Ecommerce.Services.Product
{
    public class ProductService :IProductService
    {
        private readonly IHttpContextAccessor _context;
        private readonly AppSettings _appSettings;

        public ProductService(IHttpContextAccessor context, IOptionsSnapshot<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        public async Task<PagingResult<ProductDtoV3>> GetListOfProductsAsync(Guid catalogId, int page, int pageSize)
        {
            var accessToken = await _context.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            RestClient restClient =
                new RestClient($"{_appSettings.ResourceServer.Endpoint}/api/catalog/{catalogId}/products");
            RestRequest request = new RestRequest(Method.GET);
            request.AddHeader("Accept", "application/json");
            if (_context.HttpContext.User.Identity.IsAuthenticated && !string.IsNullOrEmpty(accessToken))
                request.AddHeader("Authorization", $"Bearer {accessToken}");

            request.AddParameter("page", page);
            request.AddParameter("pageSize", pageSize);
            var rsp = await restClient.ExecuteAsync<PagingResult<ProductDtoV3>>(request);
            return rsp.IsSuccessful ? rsp.Data : default;
        }
    }
}

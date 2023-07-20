using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Ecommerce.ConfigurationOptions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace GHPCommerce.Ecommerce.Services.Catalog
{
    public class CatalogService : ICatalogService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _context;
        private readonly AppSettings _appSettings;

        public CatalogService(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor context,
            IOptionsSnapshot<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<CatalogDto>> GetCatalogsAsync()
        {
            var accessToken = await _context.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            var httpClient = _httpClientFactory.CreateClient();
            if (_context.HttpContext.User.Identity.IsAuthenticated && !string.IsNullOrEmpty(accessToken))
            {
                httpClient.SetBearerToken(accessToken);
            }
            var response = await httpClient.GetAsync($"{_appSettings.ResourceServer.Endpoint}/api/catalog/menu");
            return await response.Content.ReadAs<IEnumerable<CatalogDto>>();
        }
    }
}

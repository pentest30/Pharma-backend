using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Hpcs;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Core.Shared.Services.ExternalServices;
using GHPCommerce.CrossCuttingConcerns.Caching;
using HPCS.Service.OptionConfiguration;
using Newtonsoft.Json;
using RestSharp;
using Serilog.Core;

namespace HPCS.Service.Services
{
    public class HpcsService : IHpcsService
    {
        private readonly ICache _cache;
        private readonly AppSettings _appSettings;
        private readonly Logger _logger;
        private readonly ICallApiService _callApiService;

        public HpcsService(ICache cache, 
            AppSettings appSettings, 
            Logger logger, 
            ICallApiService callApiService)
        {
            _cache = cache;
            _appSettings = appSettings;
            _logger = logger;
            _callApiService = callApiService;
        }

        private async Task<AccessToken> AuthenticateAsync()
        {
           
            var body = new
            {
                userName = _appSettings.ExternalApiInfo.OnlineCustomerUser,
                password = _appSettings.ExternalApiInfo.OnlineCustomerPass
            };
            var apiResponse = await _callApiService.ExecuteRestRequestAsync<AccessToken>(_appSettings.ExternalApiInfo.Resource+"/api/users/signin", body, Method.POST);
            if (apiResponse.IsSuccessful)
            {
                apiResponse.Data.Date = DateTime.Now.AddSeconds(apiResponse.Data.Expiry);
               // await _cache.AddOrUpdateAsync<AccessToken>(key, apiResponse.Data);
            }
            return  apiResponse.IsSuccessful ? apiResponse.Data : new AccessToken();
        }

        private ErrorValidationContract ParseValidationErrors(Guid orderId, string content)
        {
            var validationContract = new ErrorValidationContract();
            validationContract.OrderId = orderId;
            var result = !string.IsNullOrEmpty(content)
                ? JsonConvert.DeserializeObject<ValidationResult>(content)
                : new ValidationResult();
            if (!result.Errors.Any()) return validationContract;
            int errorCode = 1;
            foreach (var validationFailure in result.Errors)
            {
                validationContract.Errors.Add(errorCode.ToString(), validationFailure.ErrorMessage);
                errorCode++;
            }
            return validationContract;
        }

       

        public async Task<IRestResponse<AtomOrderContract>> CreateOrderAsync(AtomOrderContract context)
        {
            var accessToken = await AuthenticateAsync();
            if (accessToken.Token == null)
            {
                _logger.Error("Invalid token. check the cache ...");
                throw new InvalidOperationException("invalid token, check the cache ...");
            }
            var apiResponse = await _callApiService.ExecuteRestRequestAsync<AtomOrderContract>(_appSettings.ExternalApiInfo.Resource+"/api/onlineorders/",
                new OrderCreateCommandV1 { Order = context }, Method.POST, accessToken.Token)
                .ConfigureAwait(true);
            return apiResponse;
        }

        public  async Task<IRestResponse<EditOrderContract>> EditOrderAsync(EditOrderContract message)
        {
            var accessToken = await AuthenticateAsync();
            if (accessToken.Token == null)
            {
                _logger.Error("Invalid token. check the cache ...");
                throw new InvalidOperationException("invalid token, check the cache ...");
            }
            var apiResponse = await _callApiService.ExecuteRestRequestAsync<EditOrderContract>(_appSettings.ExternalApiInfo.Resource+$"/api/onlineorders/",
                    new OrderEditCommand { Order = message }, Method.PUT, accessToken.Token)
                .ConfigureAwait(true);
            return apiResponse;
        }

        public  async Task<ErrorValidationContract> ValidateOrderAsync(ValidateOnlineOrderContract context)
        {
            var accessToken = await AuthenticateAsync();
            if (accessToken.Token == null)
            {
                _logger.Error("Invalid token. check the cache ...");
                throw new InvalidOperationException("invalid token, check the cache ...");
            }
            var apiResponse = await  _callApiService.ExecuteRestRequestAsync<ValidationResult>(_appSettings.ExternalApiInfo.Resource+$"/api/onlineorders/{context.OrderId}/{context.CustomerId}/save", null, Method.PUT,
                    accessToken.Token)
                .ConfigureAwait(true);
            return ParseValidationErrors(context.OrderId, apiResponse.Content);
        }

        public async Task<ErrorValidationContract> CancelOrderAsync(CancelOnlineOrderContract context)
        {
            var accessToken = await AuthenticateAsync();
            if (accessToken.Token == null)
            {
                _logger.Error("Invalid token. check the cache ...");
                throw new InvalidOperationException("invalid token, check the cache ...");
            }
            var apiResponse = await _callApiService.ExecuteRestRequestAsync<ValidationResult>(_appSettings.ExternalApiInfo.Resource+$"/api/onlineorders/{context.OrderId}/{context.CustomerId}/cancel", null, Method.DELETE, accessToken.Token)
                .ConfigureAwait(true);
            return  ParseValidationErrors(context.OrderId, apiResponse.Content);
        }
    }
}
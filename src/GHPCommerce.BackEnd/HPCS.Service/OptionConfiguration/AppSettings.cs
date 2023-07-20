using GHPCommerce.Infra.Cache;
using GHPCommerce.Infra.MessageBrokers;

namespace HPCS.Service.OptionConfiguration
{
    public class AppSettings
    {
        public ExternalApiInfo ExternalApiInfo { get; set; }
        public MessageBrokerOptions MessageBroker { get; set; }
        public CachingOptions Caching { get; set; }
    }

    public class ExternalApiInfo
    {
        public string OnlineCustomerUser { get; set; }
        public string OnlineCustomerPass { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Resource { get; set; }
        public string OrganizationCode { get; set; }
    }
}
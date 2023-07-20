using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Infra.Cache;
using GHPCommerce.Infra.MessageBrokers;

namespace GHPCommerce.IS4Admin.ConfigurationOptions
{
    using System.Collections.Generic;
    using ExternalLogin;
    using Microsoft.EntityFrameworkCore.Internal;

    public class AppSettings
    {
        public static ConnectionStrings ConnectionStrings { get; set; }

        public LoggingOptions Logging { get; set; }

        public CachingOptions Caching { get; set; }
        public Dictionary<string, string> SecurityHeaders { get; set; }


        public static CertificatesOptions Certificates { get; set; }

        public ExternalLoginOptions ExternalLogin { get; set; }
        public AuthMessageSenderOptions SenderOptions { get; set; }
        public MessageBrokerOptions MessageBroker { get; set; }
        public MedIJKModel MedIJKModel { get; set; }
        public OpSettings OpSettings { get; set; }
    }
}

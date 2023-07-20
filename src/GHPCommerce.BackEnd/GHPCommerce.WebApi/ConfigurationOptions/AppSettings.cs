using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Infra.Cache;
using GHPCommerce.Infra.MessageBrokers;
using GHPCommerce.Infra.Notification.Email;
using GHPCommerce.Infra.OS.Print;

namespace GHPCommerce.WebApi.ConfigurationOptions
{
    public class AppSettings
    {
        public  ConnectionStrings ConnectionStrings { get; set; }

        public IdentityServerAuthentication IdentityServerAuthentication { get; set; }

        public string AllowedHosts { get; set; }

        public CORS CORS { get; set; }
        public Dictionary<string, string> SecurityHeaders { get; set; }
        public AuthMessageSenderOptions SenderOptions { get; set; }
        public CachingOptions Caching { get; set; }
        public MessageBrokerOptions MessageBroker { get; set; }
        public PrinterOptions PrinterOptions { get; set; }
        public MedIJKModel MedIJKModel { get; set; }
        public OpSettings OpSettings { get; set; }
        public string AttachedFilesDirectoryPath { get; set; }
        public PreparationInventEndPoint PreparationInventEndPoint { get; set; }
        public DeptServiceConfig DeptServiceConfig { get; set; }
    }
}

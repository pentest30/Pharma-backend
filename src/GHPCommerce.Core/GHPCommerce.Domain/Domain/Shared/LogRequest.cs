using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Domain.Domain.Shared
{
    public class LogRequest: AggregateRoot<Guid>
    {
        public string StatusCode { get; set; }
        public string Body { get; set; } 
        public string ClientIP { get; set; }
        public int Duration { get; set; }
        public DateTime RequestTime { get; set; }
        public DateTime ResponseTime { get; set; }
        public string Header { get; set; }
        public string Methode { get; set; }
        public string Action { get; set; }
    }
}
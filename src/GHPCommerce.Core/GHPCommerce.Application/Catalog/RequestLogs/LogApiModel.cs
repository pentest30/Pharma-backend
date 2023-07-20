using System;
using GHPCommerce.Domain.Domain.Commands;
using MediatR;

namespace GHPCommerce.Application.Catalog.RequestLogs
{
    public class LogApiModel:ICommand<object>
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
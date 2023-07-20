using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Queries
{
    public class OrderLoopkupByOnlineCustomer : ICommand<List<string>>
    {
        public string CustomerCode { get; set; }
        public Guid OrganizationId { get; set; }
        public string Filter { get; set; }
    }
}

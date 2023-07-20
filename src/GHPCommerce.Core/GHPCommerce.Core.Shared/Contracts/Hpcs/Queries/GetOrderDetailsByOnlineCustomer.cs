using GHPCommerce.Core.Shared.Contracts.Hpcs;
using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Queries
{
    public class GetOrderDetailsByOnlineCustomer : ICommand<List<OrderLineModel>>
    {
        public string CustomerCode { get; set; }
        public Guid OrganizationId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime  OrderDate { get; set; }
    }
}

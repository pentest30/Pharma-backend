using GHPCommerce.Core.Shared.Contracts.Hpcs;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Queries
{
    public class GetOrdersByOnlineCustomer : ICommand<List<OrderTableModel>>
    {
        public string CustomerCode { get; set; }
        public Guid OrganizationId { get; set; } 

    }
}

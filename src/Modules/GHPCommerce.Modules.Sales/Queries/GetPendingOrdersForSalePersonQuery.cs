using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetPendingOrdersForSalePersonQuery :ICommand<IEnumerable<OrderDto>>
    {
        public Guid CustomerId { get; set; }
    }
}

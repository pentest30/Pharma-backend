using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;
using System;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetSalesPersonPendingOrderQueryV2 : ICommand<OrderDto>
    {
        public Guid CustomerId { get; set; }

    }
}

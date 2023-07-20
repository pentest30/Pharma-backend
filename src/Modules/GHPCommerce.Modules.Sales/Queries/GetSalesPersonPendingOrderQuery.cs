using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;
using System;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetSalesPersonPendingOrderQuery : ICommand<OrderDto>
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SalesPersonId { get; set; }


    }
}

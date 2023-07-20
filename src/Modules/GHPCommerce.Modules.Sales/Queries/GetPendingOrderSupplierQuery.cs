using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;
using System;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetPendingOrderSupplierQuery : ICommand<OrderDto>
    {
        public Guid SupplierId { get; set; }

    }
}

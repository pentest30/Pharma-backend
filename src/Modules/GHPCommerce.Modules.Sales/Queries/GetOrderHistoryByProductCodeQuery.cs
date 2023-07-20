using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetOrderHistoryByProductCodeQuery :ICommand<IEnumerable<OrderHistoryDto>>
    {
        public Guid CustomerId { get; set; }
        public string ProductCode { get; set; }
    }
}

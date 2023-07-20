using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetOrderByIdV1Query : ICommand<OrderDtoV2>
    {
        public Guid Id { get; set; }
    }
}
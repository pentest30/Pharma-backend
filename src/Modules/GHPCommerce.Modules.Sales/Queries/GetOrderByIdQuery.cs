using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs;
using System;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetOrderByIdQuery : ICommand<OrderDtoV2>
    {
        public Guid Id { get; set; }
    }
}

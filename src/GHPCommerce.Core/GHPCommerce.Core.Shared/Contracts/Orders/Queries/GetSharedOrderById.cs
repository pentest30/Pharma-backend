using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Queries
{
    public class GetSharedOrderById : ICommand<OrderDtoV3>
    {
        public Guid OrderId { get; set; }

    }
}

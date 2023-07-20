using System;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Queries
{
    public class GetOrderByIdQueryV2 : ICommand<OrderDtoV5>
    {
        public Guid Id { get; set; }
    }
}
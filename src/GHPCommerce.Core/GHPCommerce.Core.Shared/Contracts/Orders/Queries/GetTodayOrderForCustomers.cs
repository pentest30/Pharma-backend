using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Queries
{
    public class GetTodayOrderForCustomers : ICommand<OrderDtoV6>
    {
        public DateTime Date { get; set; }
        public Guid? CustomerId { get; set; }

    }
}

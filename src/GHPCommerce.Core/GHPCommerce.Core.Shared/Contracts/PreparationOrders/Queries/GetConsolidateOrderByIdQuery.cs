using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.PreparationOrders.Queries
{
    public class GetConsolidateOrderByIdQuery : ICommand<bool>
    {
        public Guid OrderId { get; set; }
    }
   
}
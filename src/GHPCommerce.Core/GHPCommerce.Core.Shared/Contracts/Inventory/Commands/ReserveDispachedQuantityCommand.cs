using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Commands
{
    public class ReserveDispatchedQuantityCommand:ICommand<ReserveDispatchedQuantityResponse>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public bool IsDemand { get; set; }
    }
}
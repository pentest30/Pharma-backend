using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Commands
{
    public class ReleaseDispatchedQuantityCommand : ICommand<ReserveDispatchedQuantityResponse>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}

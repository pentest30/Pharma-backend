using System;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Quota
{
    public class DecreaseQuotaAfterValidationCommand: ICommand<ReserveDispatchedQuantityResponse>
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public Guid SalesPersonId { get; set; }
        public Guid CustomerId { get; set; }
        
    }
}
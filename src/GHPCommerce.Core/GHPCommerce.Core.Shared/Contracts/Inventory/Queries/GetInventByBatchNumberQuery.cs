using System;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Queries
{
    public class GetInventByBatchNumberQuery: ICommand<InventDto>
    {
        public string InternalBatchNumber { get; set; }
        public string VendorBatchNumber { get; set; }
        public Guid ProductId { get; set; }
    }
}
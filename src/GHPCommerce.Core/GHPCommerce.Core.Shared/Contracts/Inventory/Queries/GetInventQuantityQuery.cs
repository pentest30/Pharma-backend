using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Queries
{
    public class GetInventQuantityQuery : ICommand<double>
    {
        public string InternalBatchNumber { get; set; }
        public string VendorBatchNumber { get; set; }
        public Guid ProductId { get; set; }
    }
}
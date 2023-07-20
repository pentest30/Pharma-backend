using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Batches.Queries
{
    public class GetBatchIdQuery : ICommand<Guid>
    {
        public string InternalBatchNumber { get; set; }
        public string VendorBatchNumber { get; set; }
        public Guid ProductId { get; set; }

    }
}
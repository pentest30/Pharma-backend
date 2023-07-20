using System;
using GHPCommerce.Core.Shared.Contracts.Batches.Dtos;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Batches.Queries
{
    public class GetBatchByIdQuery : ICommand<BatchDtoV1>
    {
        public string InternalBatchNumber { get; set; }
        public string VendorBatchNumber { get; set; }
        public Guid ProductId { get; set; }
    }
}
using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetQuotaProductByIdQuery : ICommand<bool>
    {
        public Guid ProductId { get; set; }
    }
}
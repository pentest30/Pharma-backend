using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Inventory;

namespace GHPCommerce.Modules.Inventory.Queries
{
    public class GetStockForSalesPerson : ICommand<List<CachedInventSum>>
    {
        public Guid SupplierId { get; set; }
        public Guid ProductId { get; set; }

    }
}

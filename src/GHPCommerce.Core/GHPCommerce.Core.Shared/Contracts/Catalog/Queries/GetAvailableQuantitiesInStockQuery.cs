using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetAvailableQuantitiesInStockQuery: ICommand<Dictionary<Guid, Tuple<int, decimal>>>
    {
        public List<Guid> ProductIds { get; set; }
        public Guid? SupplierId { get; set; }
    }
}

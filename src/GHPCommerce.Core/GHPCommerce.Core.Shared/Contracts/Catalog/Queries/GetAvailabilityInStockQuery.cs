using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetAvailabilityInStockQuery : ICommand<Dictionary<Guid, bool>>
    {
        public List<Guid> ProductIds { get; set; }
        public Guid? supplierId { get; set; }
    }
}

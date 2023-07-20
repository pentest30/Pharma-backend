using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetProductIdsByCatalogId :ICommand<IEnumerable<ProductDtoV4>>
    {
        public Guid CatalogId { get; set; }
    }
}

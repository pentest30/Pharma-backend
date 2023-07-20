using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    public class GetListOfProductsByCatalogAndNameQuery :ICommand<IEnumerable<ProductDtoV3>>
    {
        public Guid CatalogId { get; set; }
        public string SearchName { get; set; }
    }
}

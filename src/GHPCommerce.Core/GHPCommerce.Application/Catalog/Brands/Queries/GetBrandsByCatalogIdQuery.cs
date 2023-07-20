using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Brands.Queries
{
    public class GetBrandsByCatalogIdQuery : ICommand<IEnumerable<BrandDtoV1>>
    {
        public Guid CatalogId { get; set; }
    }
}

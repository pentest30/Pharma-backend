using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetListProductsByIds : ICommand<IEnumerable<ProductDtoV3>>
    {
        public List<Guid> Ids { get; set; }
    }
}

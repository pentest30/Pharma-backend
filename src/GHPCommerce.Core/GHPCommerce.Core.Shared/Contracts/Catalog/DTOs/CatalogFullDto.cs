using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.DTOs
{
    public class CatalogFullDto
    {
        public PagingResult<ProductDtoV3> PagingResult { get; set; }
        public IEnumerable<BrandDtoV1> Brands { get; set; }
            
    }
}

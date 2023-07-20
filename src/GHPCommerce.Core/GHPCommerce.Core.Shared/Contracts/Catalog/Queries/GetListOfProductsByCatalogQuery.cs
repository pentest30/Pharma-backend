using System;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    
    public class GetListOfProductsByCatalogQuery :CommonListQuery, ICommand<PagingResult<ProductDtoV3>>
    {
        public GetListOfProductsByCatalogQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
            
        }

        public Guid CatalogId { get; set; }
        public Guid? ManufacturerId { get; set; }
        public Guid? OrganizationId { get; set; }
    }
}

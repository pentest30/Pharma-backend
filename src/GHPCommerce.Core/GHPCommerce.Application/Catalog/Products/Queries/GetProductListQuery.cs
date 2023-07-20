using System;
using GHPCommerce.Application.Catalog.Products.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.Products.Queries
{
    [Obsolete("This command is not used any more, please  try to use GetPagedProductListQuery")]
    public class GetProductListQuery :CommonListQuery, ICommand<PagingResult<ProductDto>> 
    {
        public GetProductListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

using GHPCommerce.Application.Catalog.Brands.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.Brands.Queries
{
    public class GetBrandsListQuery : CommonListQuery,ICommand<PagingResult<BrandDto>> 
    {
        public GetBrandsListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

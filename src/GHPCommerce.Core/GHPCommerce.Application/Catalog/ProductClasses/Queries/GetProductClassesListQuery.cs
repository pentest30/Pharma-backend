using GHPCommerce.Application.Catalog.ProductClasses.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.ProductClasses.Queries
{
    public class GetProductClassesListQuery : CommonListQuery, ICommand<PagingResult<ProductClassDto>>
    {
        public GetProductClassesListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

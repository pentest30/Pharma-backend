using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Sales.DTOs;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetDiscountsListQuery : CommonListQuery, ICommand<PagingResult<DiscountDto>>
    {
        public GetDiscountsListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

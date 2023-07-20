using GHPCommerce.Application.Catalog.Lists.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.Lists.Queries
{
    public class GetListsListQuery : CommonListQuery, ICommand<PagingResult<ListDto>>
    {
        public GetListsListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

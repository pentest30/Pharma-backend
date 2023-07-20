using GHPCommerce.Application.Catalog.TaxGroups.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.TaxGroups.Queries
{
    public class GetTaxGroupsListQuery : CommonListQuery, ICommand<PagingResult<TaxGroupDto>>
    {
        public GetTaxGroupsListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

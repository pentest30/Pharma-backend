using GHPCommerce.Application.Catalog.INNs.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.INNs.Queries
{
    public class GetINNsListQuery :CommonListQuery, ICommand<PagingResult<InnDto>>
    {
        public GetINNsListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

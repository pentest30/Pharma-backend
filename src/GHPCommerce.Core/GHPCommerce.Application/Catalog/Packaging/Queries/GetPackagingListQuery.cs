using GHPCommerce.Application.Catalog.Packaging.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.Packaging.Queries
{
    public class GetPackagingListQuery :CommonListQuery, ICommand<PagingResult<PackagingDto>>
    {
        public GetPackagingListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Tiers.Suppliers.Queries
{
    public class GetSuppliersListQuery : CommonListQuery, ICommand<PagingResult<OrganizationDtoV1>>
    {
        public GetSuppliersListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

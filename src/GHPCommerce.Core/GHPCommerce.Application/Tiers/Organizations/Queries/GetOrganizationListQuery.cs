using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Tiers.Organizations.Queries
{
    public class GetOrganizationListQuery : CommonListQuery, ICommand<PagingResult<OrganizationDtoV1>>
    {
        public GetOrganizationListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

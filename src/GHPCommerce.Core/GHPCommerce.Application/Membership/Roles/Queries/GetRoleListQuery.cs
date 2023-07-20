using GHPCommerce.Application.Membership.Roles.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Membership.Roles.Queries
{
    public class GetRoleListQuery: CommonListQuery, ICommand<PagingResult<RoleDto>>
    {
        public GetRoleListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

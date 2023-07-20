using GHPCommerce.Application.Membership.Users.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Membership.Users.Queries
{
    public class GetUsersListQuery: CommonListQuery, ICommand<PagingResult<UserDto>>
    {
        public GetUsersListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
        }
    }
}

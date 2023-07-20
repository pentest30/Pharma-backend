using GHPCommerce.Application.Membership.Users.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Membership.Users.Queries
{
    public class GetPagedUsersQuery : ICommand<SyncPagedResult<UserDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
}
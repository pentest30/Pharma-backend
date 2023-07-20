using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Membership.Users.Queries
{
    public class GetUserIdByNameQuery : ICommand<Guid>
    {
        public string UserName { get; set; }
    }
}

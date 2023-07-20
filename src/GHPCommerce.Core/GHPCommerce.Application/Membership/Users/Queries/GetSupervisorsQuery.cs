using System.Collections.Generic;
using GHPCommerce.Application.Membership.Users.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Membership.Users.Queries
{
    public class GetSupervisorsQuery : ICommand<IEnumerable<UserDtoV1>>
    {
        public bool IncludeRoles { get; set; }
    }
    public class GetSupervisorsQueryV2 : ICommand<IEnumerable<UserDtoV1>>
    {
        public bool IncludeRoles { get; set; }
    }
}
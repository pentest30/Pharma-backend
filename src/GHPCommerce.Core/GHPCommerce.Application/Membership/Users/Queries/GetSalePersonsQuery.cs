using System.Collections.Generic;
using GHPCommerce.Application.Membership.Users.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Membership.Users.Queries
{
    public class GetSalePersonsQuery :  ICommand<IEnumerable<UserDtoV1>>
    {
        public bool IncludeRoles { get; set; }
    }
}

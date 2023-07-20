using GHPCommerce.Application.Membership.Users.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Application.Membership.Users.Queries
{
    public class GetSalesPersonsBySupervisorV2Query : ICommand<IEnumerable<UserDtoV2>>
    {
        public Guid Id { get; set; }
}
}

using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Application.Membership.Roles.Commands
{
    public class AddRoleClaimCommand :ICommand
    {
        public Guid Id { get; set; }
        public RoleClaim Claim { get; set; }
    }
}

using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Application.Membership.Roles.Commands
{
    public class AddUpdateRoleCommand : ICommand
    {
        public Role Role { get; set; }
        public Guid Id { get; set; }
    }
}

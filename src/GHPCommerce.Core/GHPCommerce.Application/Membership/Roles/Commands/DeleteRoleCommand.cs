using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Membership.Roles.Commands
{
    public class DeleteRoleCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}

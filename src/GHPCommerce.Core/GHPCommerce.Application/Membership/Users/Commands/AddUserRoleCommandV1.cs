using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Membership.Users.Commands
{
    public class AddUserRoleCommandV1:ICommand
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; }
    }
}

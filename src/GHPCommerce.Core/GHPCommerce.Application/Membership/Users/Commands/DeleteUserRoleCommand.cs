using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Application.Membership.Users.Commands
{
    public class DeleteUserRoleCommand : ICommand
    {
        public Guid Id { get; set; }
        public UserRole Role { get; set; }
    }
}

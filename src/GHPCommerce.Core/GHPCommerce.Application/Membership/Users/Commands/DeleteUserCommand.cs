using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Membership.Users.Commands
{
    public class DeleteUserCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

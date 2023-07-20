using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Shared;

namespace GHPCommerce.Application.Membership.Users.Commands
{
    public class AddUserAddressCommand: ICommand
    {
        public Guid Id { get; set; }
        public Address Address { get; set; }
    }
}

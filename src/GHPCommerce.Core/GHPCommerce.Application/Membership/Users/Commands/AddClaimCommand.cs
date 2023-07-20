using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Application.Membership.Users.Commands
{
    public class AddClaimCommand :ICommand
    {
        public UserClaim Claim { get; set; }
        public Guid Id { get; set; }
    }
}

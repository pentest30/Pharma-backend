using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Application.Membership.Users.Commands
{
    public class DeleteClaimCommand :ICommand
    {
        public Guid Id { get; set; }
        public UserClaim Claim { get; set; }
    }
}

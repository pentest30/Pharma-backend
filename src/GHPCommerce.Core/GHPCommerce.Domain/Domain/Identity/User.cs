using GHPCommerce.Domain.Domain.Common;
using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Shared;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Domain.Domain.Identity
{
    public class User : AggregateRoot<Guid>
    {
        public User()
        {
            Claims = new List<UserClaim>();
            Addresses = new List<Address>();
        }
        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string Email { get; set; }

        public string NormalizedEmail { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PasswordHash { get; set; }

        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public string ConcurrencyStamp { get; set; }

        public string SecurityStamp { get; set; }

        public bool LockoutEnabled { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }
        public Organization Organization { get; set; }
        public Guid? OrganizationId { get; set; }

        public Guid? ManagerId { get; set; }

        public int AccessFailedCount { get; set; }

        public IList<UserToken> Tokens { get; set; }

        public IList<UserClaim> Claims { get; set; }

        public IList<UserRole> UserRoles { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public List<Address>  Addresses { get; set; }

    }
}

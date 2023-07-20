using System;
using System.Collections.Generic;

namespace GHPCommerce.WebApi.Models.Users
{
    public class UserModel
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string NormalizedUserName { get; set; }

        public string Email { get; set; }

        public string NormalizedEmail { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public string ConcurrencyStamp { get; set; }

        public string SecurityStamp { get; set; }

        public bool LockoutEnabled { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public int AccessFailedCount { get; set; }

        public string Password { get; set; }
        public string OldPassword { get; set; }

        public IEnumerable<string> UserRoles { get; set; }
        public List<ClaimModel> Claims { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? ManagerId { get; set; }
    }
}

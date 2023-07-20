using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Domain.Domain.Identity
{
    public class UserRole : Entity<Guid>
    {
        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }

        public User User { get; set; }

        public Role Role { get; set; }
    }
}

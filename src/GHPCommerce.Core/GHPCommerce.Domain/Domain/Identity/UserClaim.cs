using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Domain.Domain.Identity
{
    public class UserClaim : Entity<Guid>
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public User User { get; set; }
    }
}

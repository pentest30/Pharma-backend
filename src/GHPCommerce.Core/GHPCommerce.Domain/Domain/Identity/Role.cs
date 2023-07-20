using GHPCommerce.Domain.Domain.Common;
using System;
using System.Collections.Generic;

namespace GHPCommerce.Domain.Domain.Identity
{
    public class Role : AggregateRoot<Guid>
    {
        public Role()
        {
            Claims = new List<RoleClaim>();
        }
        public virtual string Name { get; set; }

        public virtual string NormalizedName { get; set; }

        public virtual string ConcurrencyStamp { get; set; }

        public IList<RoleClaim> Claims { get; set; }

        public IList<UserRole> UserRoles { get; set; }
    }
}

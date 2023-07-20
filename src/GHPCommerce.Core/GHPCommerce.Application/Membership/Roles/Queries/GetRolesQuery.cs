using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Application.Membership.Roles.Queries
{
    public class GetRolesQuery :ICommand<IEnumerable<Role>>
    {
        public bool IncludeClaims { get; set; }
        public bool IncludeUserRoles { get; set; }
        public bool AsNoTracking { get; set; }
        public string [] ExcludedRoles { get; set; }
    }
}

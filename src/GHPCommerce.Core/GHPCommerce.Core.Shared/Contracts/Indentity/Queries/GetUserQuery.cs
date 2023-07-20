using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Core.Shared.Contracts.Indentity.Queries
{
    public class GetUserQuery :ICommand<User>
    {
        public Guid? Id { get; set; }
        public bool IncludeClaims { get; set; }
        public bool IncludeUserRoles { get; set; }
        public bool IncludeRoles { get; set; }
        public bool AsNoTracking { get; set; }
        public bool IncludeOrganization { get; set; }
    }
}

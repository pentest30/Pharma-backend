using System;
using System.Linq;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Domain.Repositories
{
    public class UserQueryOptions
    {
        public bool IncludeClaims { get; set; }
        public bool IncludeUserRoles { get; set; }
        public bool IncludeRoles { get; set; }
        public bool IncludeTokens { get; set; }
        public bool AsNoTracking { get; set; }
        public bool IncludeOrganization { get; set; }
        public bool IncludeAddresses { get; set; }
    }

    public interface IUserRepository : IRepository<User, Guid>
    {
        IQueryable<User> Get(UserQueryOptions queryOptions);
    }
}

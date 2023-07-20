using System;
using System.Linq;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Persistence.Repositories
{
    public class RoleRepository : Repository<Role, Guid>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
            : base(dbContext, dateTimeProvider)
        {
        }

        public IQueryable<Role> Get(RoleQueryOptions queryOptions)
        {
            var query = Table;

            if (queryOptions.IncludeClaims)
            {
                query = query.Include(x => x.Claims);
            }

            if (queryOptions.IncludeUserRoles)
            {
                query = query.Include(x => x.UserRoles);
            }

            if (queryOptions.IncludeUsers)
            {
                query = query.Include("UserRoles.User");
            }

            if (queryOptions.AsNoTracking)
            {
                query = query.AsNoTracking();
            }

            return query;
        }
    }
}

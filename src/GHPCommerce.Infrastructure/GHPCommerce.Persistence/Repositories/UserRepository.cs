using System;
using System.Linq;
using GHPCommerce.CrossCuttingConcerns.OS;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Persistence.Repositories
{
    public class UserRepository : Repository<User, Guid>, IUserRepository
    {
        public UserRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
            : base(dbContext, dateTimeProvider)
        {
        }

        public IQueryable<User> Get(UserQueryOptions queryOptions)
        {
            var query = Table;
            if (queryOptions.IncludeTokens)
            {
                query = query.Include(x => x.Tokens);
            }

            if (queryOptions.IncludeClaims)
            {
                query = query.Include(x => x.Claims);
            } 

            if (queryOptions.IncludeUserRoles)
            {
                query = query.Include(x => x.UserRoles);
            }

            if (queryOptions.IncludeRoles)
            {
                query = query.Include("UserRoles.Role");
            }
            if (queryOptions.IncludeAddresses)
            {
                query = query.Include(x=>x.Addresses);
            }

            if (queryOptions.AsNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (queryOptions.IncludeOrganization)
            {
                query = query.Include("Organization");
            }

            return query;
        }
    }
}

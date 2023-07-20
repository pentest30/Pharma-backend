using System;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
namespace GHPCommerce.Core.Shared.Services
{
    public class OrganizationService : ICurrentOrganization
    {
        private readonly ICurrentUser _user;
        private readonly ICommandBus _commandBus;
        private readonly ICache _redisCache;

        public OrganizationService(ICurrentUser user , ICommandBus commandBus, ICache redisCache)
        {
            _user = user;
            _commandBus = commandBus;
            _redisCache = redisCache;
        }

      
        public async Task<Guid?> GetCurrentOrganizationIdAsync()
        {
           if (_user.UserId == Guid.Empty) return null;
            var currentOrgId = await _redisCache
                .GetAsync<string>($"organizationId_{_user.UserId}")
                .ConfigureAwait(true);
            if (currentOrgId != null&& currentOrgId!=Guid.Empty.ToString()) return Guid.Parse(currentOrgId);
            // if (currentOrgId != null&& currentOrgId==Guid.Empty.ToString()) return null;
            var query = await _commandBus.SendAsync(new GetOrganizationIdByUserQuery {UserId = _user.UserId});
            await _redisCache.AddOrUpdateAsync<string>($"organizationId_{_user.UserId}",query.HasValue? query.ToString(): Guid.Empty.ToString());
            return query;
            //return null;
        }
        public async Task<string> GetCurrentOrganizationNameAsync()
        {
            if (_user.UserId == Guid.Empty) return null;
            var query = await _commandBus.SendAsync(new GetUserQuery { Id = _user.UserId,IncludeOrganization = true});
            return query.Organization?.Name;
        }
    }
}

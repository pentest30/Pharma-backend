using System.Collections.Generic;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GHPCommerce.IS4Admin.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;

        public NotificationHub(ICurrentOrganization organization, ICurrentUser currentUser /*ICache redisCache*/)
        {
            _currentOrganization = organization;
            _currentUser = currentUser;
            //_redisCache = redisCache;
        }

        public override async Task OnConnectedAsync()
        {

            var id = _currentUser.UserId.ToString();
            var orgId  =await _currentOrganization.GetCurrentOrganizationIdAsync();
            var current = await _redisCache.GetAsync<Dictionary<string, string>>("_signalR_" +
                orgId);
            if (current == null && orgId!=null)
            {
                var dic = new Dictionary<string, string>();
                dic.Add(id, Context.ConnectionId);
                await _redisCache.AddOrUpdateAsync<Dictionary<string, string>>("_signalR_" + orgId, dic);
            }
            else if(orgId != null)
            {
                current[id] = Context.ConnectionId;
                await _redisCache.AddOrUpdateAsync<Dictionary<string, string>>( "_signalR_" + orgId, current);
            }

            await base.OnConnectedAsync();
        }

        public void JoinGroup(string orgId)
        {
             Groups.AddToGroupAsync(Context.ConnectionId, orgId).GetAwaiter().GetResult();
        }
    }
}

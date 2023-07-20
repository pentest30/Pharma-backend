using System;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GHPCommerce.Modules.Procurement.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProcurementHub : Hub
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;

        public ProcurementHub(ICurrentUser currentUser, ICache redisCache)
        {
            _currentUser = currentUser;
            _redisCache = redisCache;
        }
        public override Task OnConnectedAsync()
        {
            _redisCache.AddOrUpdate<string>("proc_hub" + _currentUser.UserId, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _redisCache.Expire<string>("proc_hub" + _currentUser.UserId);

            return base.OnDisconnectedAsync(exception);
        }
    }
}
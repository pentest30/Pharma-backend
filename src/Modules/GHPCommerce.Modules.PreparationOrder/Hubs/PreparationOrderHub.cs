using System;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GHPCommerce.Modules.PreparationOrder.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PreparationOrderHub: Hub
    {
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;

        public PreparationOrderHub(ICurrentUser currentUser, ICache redisCache)
        {
            _currentUser = currentUser;
            _redisCache = redisCache;
        }
        public override Task OnConnectedAsync()
        {
            _redisCache.AddOrUpdate<string>("op_hub" + _currentUser.UserId, Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _redisCache.Expire<string>("op_hub" + _currentUser.UserId);

            return base.OnDisconnectedAsync(exception);
        }
    }
}
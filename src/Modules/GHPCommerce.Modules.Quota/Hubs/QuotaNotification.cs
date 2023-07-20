using System;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GHPCommerce.Modules.Quota.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class QuotaNotification : Hub
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;

        public QuotaNotification(ICommandBus commandBus, ICurrentUser currentUser, ICache redisCache)
        {
            _commandBus = commandBus;
            _currentUser = currentUser;
            _redisCache = redisCache;
        }
        public string GetConnectionId()
        {
           // await _redisCache.AddOrUpdateAsync<string>(_currentUser.UserId.ToString(), Context.ConnectionId);
            var id = _currentUser.UserId;
            return Context.ConnectionId;

        }

        public override   Task OnConnectedAsync()
        {
             _redisCache.AddOrUpdateAsync<string>(_currentUser.UserId.ToString(), Context.ConnectionId);
            return  base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _redisCache.ExpireAsync<string>(_currentUser.UserId.ToString());
            return base.OnDisconnectedAsync(exception);
        }
    }
}
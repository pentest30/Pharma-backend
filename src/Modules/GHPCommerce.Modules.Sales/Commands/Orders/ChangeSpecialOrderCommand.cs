using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class ChangeSpecialOrderCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public bool IsSpecial { get; set; }
        public Guid CustomerId { get; set; }
    }
    public  class  ChangeSpecialOrderCommandHandler : ICommandHandler<ChangeSpecialOrderCommand, ValidationResult>
    {
        private readonly ICache _redisCache;
        private readonly ICurrentUser _currentUser;

        public ChangeSpecialOrderCommandHandler(ICache redisCache, ICurrentUser currentUser)
        {
            _redisCache = redisCache;
            _currentUser = currentUser;
        }
        public async Task<ValidationResult> Handle(ChangeSpecialOrderCommand request, CancellationToken cancellationToken)
        {
            var key = request.CustomerId.ToString() + _currentUser.UserId + request.Id;
            var draftOrder = await _redisCache.GetAsync<CachedOrder>(key, cancellationToken);
            if (draftOrder == null) return default;
            draftOrder.IsSpecialOrder = !draftOrder.IsSpecialOrder; 
            await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder , cancellationToken);
            return default;
        }
    }
}
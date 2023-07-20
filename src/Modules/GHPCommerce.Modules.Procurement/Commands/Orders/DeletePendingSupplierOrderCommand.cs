using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore.Internal;
using NLog;

namespace GHPCommerce.Modules.Procurement.Commands.Orders
{
    public class DeletePendingSupplierOrderCommand : ICommand<ValidationResult>
    {
        public Guid OrderId { get; set; }
    }
    public class DeletePendingSupplierOrderCommandHandler : ICommandHandler<DeletePendingSupplierOrderCommand, ValidationResult>
    {
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;
        private readonly Logger _logger;
        private const string CACHE_KEY = "_supplier-order";

        public DeletePendingSupplierOrderCommandHandler(ICurrentOrganization currentOrganization,
            ICommandBus commandBus, 
            ICurrentUser currentUser, 
            ICache redisCache)
        {
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _redisCache = redisCache;
        }
        public async Task<ValidationResult> Handle(DeletePendingSupplierOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == default) throw new InvalidOperationException("");
                var lookupKey = await GetLookupKey();

                var lookUp = _redisCache.Get<List<Guid>>(lookupKey);
                string key = await GetOrderKey(request.OrderId);

                var draftOrder = _redisCache.Get<CachedOrder>(key);
                if (draftOrder == null)
                    throw new InvalidOperationException("Commande non trouvée");
                lookUp?.Remove(draftOrder.Id);
                if (lookUp != null && lookUp.Any())
                    await _redisCache.AddOrUpdateAsync<List<Guid>>(key, lookUp, cancellationToken);
                else await _redisCache.ExpireAsync<List<Guid>>(key, cancellationToken);
                await _redisCache.ExpireAsync<CachedOrder>(key, cancellationToken);
                return default!;
            }
            catch (Exception ex)
            {
                var validations = new ValidationResult
                    {Errors = {new ValidationFailure("Transaction rolled back", ex.Message)}};
                return validations;
            }
          
        }
        private async Task<string> GetOrderKey( Guid orderId)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            return orgId.ToString() + _currentUser.UserId + orderId;
        }
        private async Task<string> GetLookupKey()
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            return orgId.ToString() + _currentUser.UserId + CACHE_KEY;
        }
    }
}
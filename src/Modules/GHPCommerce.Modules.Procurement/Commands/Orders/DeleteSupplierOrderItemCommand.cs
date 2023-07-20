using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using NLog;

namespace GHPCommerce.Modules.Procurement.Commands.Orders
{
    public class DeleteSupplierOrderItemCommand : ICommand<ValidationResult>
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
    }
    public  class DeleteSupplierOrderItemCommandHandler : ICommandHandler<DeleteSupplierOrderItemCommand, ValidationResult>
    {
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;
        private readonly Logger _logger;
        private const string CACHE_KEY = "_supplier-order";
        
        public DeleteSupplierOrderItemCommandHandler(ICurrentOrganization currentOrganization, 
            ICommandBus commandBus, 
            ICurrentUser currentUser, 
            ICache redisCache, 
            Logger logger)
        {
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _redisCache = redisCache;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(DeleteSupplierOrderItemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == default) throw new InvalidOperationException("");
                var key = await GetOrderKey(request.OrderId);
                var draftOrder  = _redisCache.Get<CachedOrder>(key);
                if (draftOrder == null)
                    throw new InvalidOperationException("Commande non trouvée");
                var index = draftOrder.OrderItems.FindIndex(x => x.ProductId == request.ProductId);
                if (index < 0)
                    throw new InvalidOperationException("Ligne commande non trouvée");
                draftOrder.OrderItems.RemoveAt(index);
                await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
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
    }
}
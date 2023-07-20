using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Sales.Models;
using NLog;

namespace GHPCommerce.Modules.Sales.Commands.Atom
{
    public class CancelOnlineOrderCommand : ICommand<ValidationResult>
    {
        public string CustomerCode { get; set; }
        public Guid OrderId { get; set; }
    }
    public class CancelOnlineOrderCommandHandler : ICommandHandler<CancelOnlineOrderCommand, ValidationResult>
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly Logger _logger;
        private readonly ICache _redisCache;

        public CancelOnlineOrderCommandHandler(ICommandBus commandBus,
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser,
            Logger logger, 
            ICache redisCache)
        {
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _logger = logger;
            _redisCache = redisCache;
        }
        public async Task<ValidationResult> Handle(CancelOnlineOrderCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            try
            {
                if (orgId == null)
                {
                    throw new InvalidOperationException("Organization non valide");
                }
                var activeCustomer = await _commandBus.SendAsync(new GetCustomerByCodeQueryV2
                {
                    Code = request.CustomerCode, 
                    SupplierOrganizationId = orgId.Value
                }, cancellationToken);
                if (activeCustomer?.CustomerId == null)
                {
                    throw new InvalidOperationException("Client non valide");
                }
                
                var userId = activeCustomer.SalesPersonId ?? _currentUser.UserId;
                var key = activeCustomer.CustomerId + userId.ToString() + request.OrderId;
                var draftOrder = await _redisCache.GetAsync<CachedOrder>(key, cancellationToken);
                if (draftOrder == null)
                {
                    throw new InvalidOperationException("Commande validée ou supprimée");
                }

                foreach (CachedOrderItem orderItem in draftOrder.OrderItems)
                {
                    await ReleaseQuantities(orderItem, orgId.Value);
                    var cmd = new ReleaseReservedQuantityCommandV2
                    {
                        InternalBatchNumber = orderItem.InternalBatchNumber, 
                        ProductId = orderItem.ProductId,
                        Quantity = orderItem.Quantity
                    };
                    await _commandBus.SendAsync(cmd, cancellationToken);
                }

                await DeletePendingOrderByIdAsync(draftOrder.Id, cancellationToken);
                await _redisCache.ExpireAsync<CachedOrder>(key , cancellationToken).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                var  validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                _logger.Error( nameof(CancelOnlineOrderCommand)+": "+ ex.Message);
                _logger.Error(ex.InnerException?.Message);
                return validations;
            }

            return default;
        }
        private async Task ReleaseQuantities(CachedOrderItem orderItem, Guid supplierId)
        {
            var key = orderItem.ProductId.ToString() + supplierId;
            try
            {
                await LockProvider<string>.ProvideLockObject(key).WaitAsync();
                var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key);
                var index = inventSum.CachedInventSumCollection.CachedInventSums
                    .FindIndex(x =>
                        x.ProductId == orderItem.ProductId
                        && x.InternalBatchNumber == orderItem.InternalBatchNumber);
                if (index == -1)
                {
                    return;
                }

                if (inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity >=
                    orderItem.Quantity)
                    inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity -=
                        orderItem.Quantity;
                else
                    inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity = 0;
                
                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
                //throw;
            }
            finally
            {
                LockProvider<string>.ProvideLockObject(key).Release();
            }
            
        }
        private async Task DeletePendingOrderByIdAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var listOfPendingOrders =
                await _redisCache.GetAsync<List<PendingOrdersModel>>("pending_orders", cancellationToken);
            if (listOfPendingOrders != null)
            {
                var item = listOfPendingOrders.FirstOrDefault(x => x.Id == orderId);
                if (item != null)
                {
                    listOfPendingOrders.Remove(item);
                    await _redisCache.AddOrUpdateAsync<List<PendingOrdersModel>>("pending_orders", listOfPendingOrders,
                        cancellationToken);
                }
            }
        }
    }
}
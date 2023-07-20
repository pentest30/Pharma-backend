using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders.Common;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Queries;
using NLog;

namespace GHPCommerce.Modules.Sales.Commands.Atom
{
    public class OrderEditCommandHandler : ICommandHandler<OrderEditCommand, EditOrderContract>
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly Logger _logger;
        private readonly ICache _redisCache;
        private readonly IServiceReleaseQuantities _serviceReleaseQuantities;
        private static string _key;

        public OrderEditCommandHandler(ICommandBus commandBus,
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser,
            Logger logger, 
            ICache redisCache, 
            IServiceReleaseQuantities serviceReleaseQuantities)
        {
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _logger = logger;
            _redisCache = redisCache;
            _serviceReleaseQuantities = serviceReleaseQuantities;
        }

        public async Task<EditOrderContract> Handle(OrderEditCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            try
            {
                if (orgId == null)
                {
                    throw new InvalidOperationException("Organization non valide");
                }

                var customer = await _commandBus.SendAsync(new GetCustomerByCodeQueryV3
                {
                    OrganizationCode = request.Order.SupplierId,
                    CustomerCode = request.Order.CustomerId
                }, cancellationToken);
                if (customer?.CustomerId == null)
                    throw new InvalidOperationException("Client non valide");
                if (customer.CustomerState == CustomerState.Blocked)
                    throw new InvalidOperationException("Client bloqué");
                var userId = customer.SalesPersonId ?? _currentUser.UserId;
                _key = customer.CustomerId + userId.ToString() + request.Order.OrderId;
                await LockProvider<string>.ProvideLockObject(_key).WaitAsync(cancellationToken);
                var draftOrder = await _redisCache.GetAsync<CachedOrder>(_key, cancellationToken);
                if (draftOrder == null)
                {
                    throw new InvalidOperationException("Commande  Non trouvée");
                }

                foreach (var orderItem in request.Order.EditOrderItems.Where(x => x.Quantity != 0))
                {
                    var item = draftOrder.OrderItems
                        .FirstOrDefault(x => x.InternalBatchNumber == orderItem.BatchNumber && x.ProductCode == orderItem.ProductCode);
                    if (item == null) continue;
                    await ReleaseQuantities(item, orderItem, orgId, cancellationToken);
                    int newQnt = 0;
                    if (item.Quantity + orderItem.Quantity == 0)
                    {
                        newQnt = item.Quantity;
                        draftOrder.OrderItems.Remove(item);
                    }
                    else
                    {
                        newQnt = item.Quantity+ orderItem.Quantity;
                        item.Quantity += orderItem.Quantity;
                        await ApplyDiscount(cancellationToken, orgId, item);
                    }
                    // adding the difference between the old quantity and the new quantity to the quota
                    if (customer.SalesPersonId != null)
                    {
                        await IncreaseQuota(cancellationToken, item, newQnt, customer.SalesPersonId.Value, draftOrder);
                    }
                } 
               
                request.Order.TotalNetAmount = draftOrder.OrderItems.CalculateTotalExlDiscountTax();
                request.Order.EditOrderItems = new List<EditOrderItem>();
                request.Order.EditOrderItems
                    .AddRange(draftOrder.OrderItems
                    .Select(x => 
                        new EditOrderItem
                            {
                                ProductCode = x.ProductCode,
                                BatchNumber = x.InternalBatchNumber,
                                Quantity = x.Quantity, 
                                UnitPrice = x.UnitPrice,
                                ExpiryDate = x.ExpiryDate, 
                                ProductName = x.ProductName
                            }
                    ));
                await _redisCache.AddOrUpdateAsync<CachedOrder>(_key, draftOrder, cancellationToken) .ConfigureAwait(true);
                if (draftOrder.OrderItems.Any())
                {
                    await _commandBus.SendAsync(new ValidateOnlineOrderCommand{ CustomerCode = request.Order.CustomerId, OrderId = request.Order.OrderId }, cancellationToken);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(nameof(ValidateOnlineOrderCommand) + ": " + ex.Message);
                _logger.Error(ex.InnerException?.Message);
                request.Order.EditOrderItems = new List<EditOrderItem>();
            }
            finally
            {
                if (!string.IsNullOrEmpty(_key))
                    LockProvider<string>.ProvideLockObject(_key).Release();
            }

            return request.Order;
            
        }

        private async Task IncreaseQuota(CancellationToken cancellationToken, CachedOrderItem item, int newQnt,Guid salesPersonId, CachedOrder draftOrder)
        {
            await _commandBus.SendAsync(new IncreaseQuotaCommand
                {
                    ProductId = item.ProductId,
                    Quantity = newQnt,
                    CustomerId = draftOrder.CustomerId.Value,
                    SalesPersonId = salesPersonId
                },
                cancellationToken);
        }

        private async Task ApplyDiscount(CancellationToken cancellationToken, Guid? orgId, CachedOrderItem item)
        {
           
            var discount = await GetDiscountByProductAsync(orgId, item.ProductId, item.Quantity, cancellationToken);
            if (discount != null)
            {
                item.Discount = (double)discount.DiscountRate;
            }
        }

        private async Task<DiscountDto> GetDiscountByProductAsync( Guid? orgId,Guid productId,int quantity, CancellationToken cancellationToken)
        {
            var discount = (await _commandBus.SendAsync(new GetActiveDiscountByProductQuery
                {
                    OrganizationId = orgId,
                    ProductId = productId
                }, cancellationToken))
                .LastOrDefault(disc => disc.ThresholdQuantity >= quantity);
            return discount;
        }

        private async Task ReleaseQuantities(CachedOrderItem item, EditOrderItem orderItem, Guid? orgId,CancellationToken cancellationToken)
        {
            await _serviceReleaseQuantities.ReleaseQuantities(item.ProductId, item.InternalBatchNumber,orderItem.Quantity * -1, orgId.Value);
            var cmd = new ReleaseReservedQuantityCommandV2
            {
                InternalBatchNumber = item.InternalBatchNumber,
                ProductId = item.ProductId,
                Quantity = orderItem.Quantity * -1
            };
            await _commandBus.SendAsync(cmd, cancellationToken);
           
        }
    }
}
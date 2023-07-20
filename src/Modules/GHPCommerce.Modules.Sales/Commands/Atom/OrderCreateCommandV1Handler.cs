using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders.Common;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Models;
using GHPCommerce.Modules.Sales.Queries;
using NLog;

namespace GHPCommerce.Modules.Sales.Commands.Atom
{
    public class OrderCreateCommandV1Handler : ICommandHandler<OrderCreateCommandV1, AtomOrderContract>
    {
        private readonly ICache _cache;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly Logger _logger;
        static string  _key = String.Empty;
        public OrderCreateCommandV1Handler(ICache cache,
            ICommandBus commandBus,
            ICurrentOrganization currentOrganization, 
            ICurrentUser currentUser,
            Logger logger)
        {
            _cache = cache;
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _logger = logger;
        }
        public async Task<AtomOrderContract> Handle(OrderCreateCommandV1 request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            
            try
            {
                if (orgId == null)
                {
                    throw new InvalidOperationException("Organization non valide");
                }
                var activeCustomer = await _commandBus.SendAsync(new GetCustomerByCodeQueryV3
                {
                    OrganizationCode = request.Order.SupplierId,
                    CustomerCode = request.Order.CustomerId
                }, cancellationToken);
                if (activeCustomer?.CustomerId == null)
                    throw new InvalidOperationException("Client non valide");
                if (activeCustomer.CustomerState == CustomerState.Blocked)
                    throw new InvalidOperationException("Client bloqué");
                var userId = activeCustomer.SalesPersonId ?? _currentUser.UserId;
                _key = activeCustomer.CustomerId + userId.ToString() + request.Order.OrderId;
                await LockProvider<string>.ProvideLockObject(_key).WaitAsync(cancellationToken);
                var draftOrder = await _cache.GetAsync<CachedOrder>(_key, cancellationToken);
                if (draftOrder != null)
                {
                    return GetReservedOrder(request, draftOrder);
                }

                var lineReserved = 0;

                var lastOrderCmd = default(OrderItemCreateCommand);
                foreach (var atomOrderItem in request.Order.Items)
                {

                    var product = await _commandBus
                        .SendAsync(new GetProductByCode { CodeProduct = atomOrderItem.ProductCode }, cancellationToken)
                        .ConfigureAwait(true);
                    if (product == null || product .ProductState == ProductState.Deactivated) continue;
                    // In case the product is tagged as quota.
                    if (product.HasQuota)
                    {
                        // default  sales person should not be null
                        if (activeCustomer.SalesPersonId != null)
                        {
                            // verify if the current customer has sufficient quantity.
                            var qnt = await CheckAvailableQuantityForQuota(activeCustomer.CustomerId.Value, product.Id, activeCustomer.SalesPersonId.Value, atomOrderItem.Quantity);
                            // if the remaining quota quantity is less than or equal to zero, the reservation should be ignored.
                            if (qnt <= 0)
                                continue;
                            // if the requested quantity is greater than the remaining quantity.
                            if (atomOrderItem.Quantity > qnt)
                                atomOrderItem.Quantity = qnt;
                        }
                        // otherwise, disregard the reservation for this particular product.
                        else
                        {
                            continue;
                        }
                    }
                    // creating an instance of the order item.
                    var cmd = GetOrderItemCommand(request, atomOrderItem, product, activeCustomer, orgId);
                    if (cmd == default) continue;
                    // try to reserve quantities using FIFO algorithm
                    var res = await _commandBus.SendAsync(cmd, cancellationToken).ConfigureAwait(true);
                    
                    if (res != null) continue;
                    lineReserved++;
                    lastOrderCmd = cmd;

                }
                if (lastOrderCmd != default)
                {
                    await AddPendingOrdersListAsync(lastOrderCmd, userId, cancellationToken);
                }

                if (lineReserved > 0)
                {
                    draftOrder = await _cache.GetAsync<CachedOrder>(_key, cancellationToken);
                    if (draftOrder != null)
                    {
                        foreach (var draftOrderOrderItem in draftOrder.OrderItems)
                        {
                            // Updating the consumed quota by reducing the quantity. 
                            await _commandBus.SendAsync(new DecreaseQuotaCommand
                                    { ProductId = draftOrderOrderItem.ProductId, Quantity = draftOrderOrderItem.Quantity, CustomerId = draftOrder.CustomerId.Value, SalesPersonId = activeCustomer.SalesPersonId.Value},
                                cancellationToken).ConfigureAwait(false);
                            // apply discount value
                            var discount =await GetDiscountByProductAsync(orgId, draftOrderOrderItem.ProductId, draftOrderOrderItem.Quantity, cancellationToken);
                            if (discount != null)
                            {
                                draftOrderOrderItem.Discount = (double)discount.DiscountRate;
                            }
                        }

                        draftOrder.OrderDiscount = draftOrder.OrderItems.CalculateTotalIncludeDiscount();
                        draftOrder.OrderTotal = draftOrder.OrderItems.CalculateTotalIncTax();
                        var totalNet = draftOrder.OrderItems.CalculateTotalExlDiscountTax();
                        request.Order.TotalNetAmount =totalNet;
                        var finalOrder = GetReservedOrder(request, draftOrder);
                        await _cache.AddOrUpdateAsync<CachedOrder>(_key, draftOrder, cancellationToken);
                        return finalOrder;
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(nameof(OrderCreateCommandV1)+":"+ e.Message);
                _logger.Error(e.InnerException?.Message);
            }
            finally
            {
                if (!string.IsNullOrEmpty(_key))
                {
                    LockProvider<string>.ProvideLockObject(_key).Release();
                }
                
            }

            return request.Order;
        }

        private async Task<int> CheckAvailableQuantityForQuota(Guid customerId, Guid productId, Guid activeCustomerSalesPersonId, int quantity)
        {
            var qnt =await _commandBus.SendAsync(new GetQuotasForSalesPersonByProductIdQuery { ProductId = productId, SalesPersonId = activeCustomerSalesPersonId , CustomerId = customerId, Quantity = quantity});
            return qnt;
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

        private async Task AddPendingOrdersListAsync(OrderItemCreateCommand request, Guid userId,
            CancellationToken cancellationToken)
        {
            var pendingOrders = await _cache.GetAsync<List<PendingOrdersModel>>("pending_orders", cancellationToken);
            if (pendingOrders == null)
            {
                await _cache.AddOrUpdateAsync<List<PendingOrdersModel>>("pending_orders",
                    new List<PendingOrdersModel>
                    {
                        new PendingOrdersModel
                        {
                            Id = request.OrderId,
                            CustomerId = request.CustomerId,
                            SalesPersonId = userId
                        }
                    }, cancellationToken);
            }
            else
            {
                if (pendingOrders.All(x => x.Id != request.OrderId))
                {
                    pendingOrders.Add(new PendingOrdersModel
                    {
                        Id = request.OrderId,
                        CustomerId = request.CustomerId,
                        SalesPersonId = userId
                    });
                    await _cache.AddOrUpdateAsync<List<PendingOrdersModel>>("pending_orders", pendingOrders,
                        cancellationToken);
                    
                }
            }
        }


        private static AtomOrderContract GetReservedOrder(OrderCreateCommandV1 request, CachedOrder draftOrder)
        {
            var finalOrder = request.Order.DeepCloneJson();
            finalOrder.OrderId = draftOrder.Id;
            finalOrder.OrderDate = draftOrder.OrderDate;
            foreach (var atomOrderItem in request.Order.Items)
            {
                var items = draftOrder.OrderItems.Where(x =>
                    x.ProductCode == atomOrderItem.ProductCode);
                var cachedOrderItems = items as CachedOrderItem[] ?? items.ToArray();
                if (cachedOrderItems.Any())
                {
                    var currentItem = finalOrder.Items.FirstOrDefault(x =>x.ProductCode == atomOrderItem.ProductCode);
                    if (currentItem != null)
                    {
                        finalOrder.Items.Remove(currentItem);
                    }
                    foreach (var cachedOrderItem in cachedOrderItems)
                    {
                        finalOrder.Items.Add(new AtomOrderItem
                            {
                                BatchNumber = cachedOrderItem.InternalBatchNumber,
                                Quantity = cachedOrderItem.Quantity,
                                DiscValue = (decimal)cachedOrderItem.Discount +
                                            cachedOrderItem.ExtraDiscount,
                                ExpiryDate = cachedOrderItem.ExpiryDate,
                                ProductCode = cachedOrderItem.ProductCode,
                                UnitPrice = cachedOrderItem.UnitPrice,
                            }
                        );
                    }
                }
            }

            return finalOrder;
        }

        private static OrderItemCreateCommand GetOrderItemCommand(OrderCreateCommandV1 request, AtomOrderItem atomOrderItem, ProductDtoV3 product, CustomerDtoV1 activeCustomer, Guid? orgId)
        {
            if (activeCustomer.CustomerId == null) return default;
            if (orgId == null) return default;
            var cmd = new OrderItemCreateCommand
            {
                RefDocumentHpcs = request.Order.RefDoc,
                DateDocumentHpcs = DateTime.Now,
                MinExpiryDate = DateTime.MinValue,
                ProductCode = atomOrderItem.ProductCode,
                ProductId = product.Id,
                CustomerId = activeCustomer.CustomerId.Value,
                InternalBatchNumber = String.Empty,
                Quantity = atomOrderItem.Quantity,
                OrderType = (OrderType)(request.Order.Psychotropic ? 1 : 0),
                SupplierOrganizationId = orgId.Value,
                SalesPersonId = activeCustomer.SalesPersonId,
                OrderId = request.Order.OrderId, 
                PickingZoneId = product.PickingZoneId,
                DefaultLocation = product.DefaultLocation, 
                ZoneGroupId = product.ZoneGroupId,
                PickingZoneName = product.PickingZoneName,
                ZoneGroupName = product.ZoneGroupName, 
                PickingZoneOrder = product.PickingZoneOrder,
                Tax = Convert.ToDouble(product.Tax)
            };
            return cmd;

        }
    }
}
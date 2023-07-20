using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders.Common;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Commands.Orders;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Hubs;
using GHPCommerce.Modules.Sales.Models;
using GHPCommerce.Modules.Sales.Queries;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace GHPCommerce.Modules.Sales.Commands.OnlineOrder
{
    public class OnlineOrderCommandHandler :
        ICommandHandler<OrderCreateCommand, CachedOrder>,
        ICommandHandler<OrderCreateCommandV2, CachedOrder>

    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
      
        private InventSumCreatedEvent _initialInventSums;
        private readonly ICache _redisCache;
        private readonly IHubContext<InventSumHub> _hubContext;
        private readonly Logger _logger;
        private readonly MedIJKModel _model;
        public OnlineOrderCommandHandler(IOrdersRepository ordersRepository,
            IMapper mapper,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICurrentUser currentUser,
            ICache redisCache,
            IHubContext<InventSumHub> hubContext, Logger logger, MedIJKModel model)
        {
            _ordersRepository = ordersRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _redisCache = redisCache;
            _hubContext = hubContext;
            _logger = logger;
            _model = model;
        }
        public async Task<CachedOrder> Handle(OrderCreateCommand request, CancellationToken cancellationToken)
        {
          
            var _list = new List<OrderItemCreateCommand>();
            var activeCustomer = (await _commandBus.SendAsync(new GetCustomerByCodeQueryV2()
            {
                SupplierOrganizationId = request.OnlineOrderRequest.SupplierId,
                Code = request.OnlineOrderRequest.Code
            }));
            if (activeCustomer == null)
            {
#region send , you're not a valid customer yet for this supplier
#endregion
                throw new Exception("Client non valide");
                   
            }
            if (activeCustomer.CustomerState == GHPCommerce.Domain.Domain.Tiers.CustomerState.Blocked)
            {
#region send , you're blocked for ordering
#endregion
                throw new Exception("Client bloqué");
            }
            Guid customerId = activeCustomer.CustomerId.HasValue ? activeCustomer.CustomerId.Value : Guid.Empty;
            if (customerId == Guid.Empty)
            {
#region send , you're not a valid customer yet for this supplier
#endregion
                throw new Exception("Client non valide");
            }
            foreach( var _ in request.OnlineOrderRequest.OrderItems)
            {
                Guid productId = _.ProductId;
                var product = (await _commandBus.SendAsync(new GetProductByCode()
                {
                    CodeProduct = _.ProductCode
                }).ConfigureAwait(true));
                if (product != null)
                    _list.Add(
                        new OrderItemCreateCommand()
                        {
                            RefDocumentHpcs = request.OnlineOrderRequest.RefDocumentHpcs,
                            DateDocumentHpcs = request.OnlineOrderRequest.DateDocumentHpcs,
                            MinExpiryDate = DateTime.MinValue,
                            ProductCode = _.ProductCode,
                            ProductId = product.Id,
                            CustomerId = customerId,
                            InternalBatchNumber = _.InternalBatchNumber,
                            Quantity = _.Quantity,
                            OrderType = (GHPCommerce.Core.Shared.Enums.OrderType)(request.OnlineOrderRequest.Psychotropic ? 1 : 0),
                            SupplierOrganizationId = request.OnlineOrderRequest.SupplierId,
                            SalesPersonId = activeCustomer.SalesPersonId,
                            OrderId=request.OnlineOrderRequest.Id                         
                        }
                        );
            } 
            bool success = false;  
            foreach(var _ in _list)
            {
                var res = await _commandBus.SendAsync(_).ConfigureAwait(true); 
                if (res == null) success = true;
            };
            if (!success) return null;

            #region get created order
            GetSalesPersonPendingOrderQuery query = new GetSalesPersonPendingOrderQuery()
            {
                CustomerId= activeCustomer.CustomerId.Value,
                SalesPersonId= activeCustomer.SalesPersonId??Guid.Empty,
                OrderId=request.OrderId
            };
            var result = await _commandBus.SendAsync(query);
            await AddPendingOrdersListAsync(_list.First(), cancellationToken);
            var draftOrder = await ApplyDiscounts(result,activeCustomer.CustomerId.Value,activeCustomer.SalesPersonId.Value,request.SupplierOrganizationId, cancellationToken);
            
            return draftOrder;
#endregion
        
        }

        public async Task<CachedOrder> Handle(OrderCreateCommandV2 request, CancellationToken cancellationToken)
        {
            bool success = false;
            try
            {
                foreach(var _ in request.orderItems)
                {
                    var res = await _commandBus.SendAsync(_).ConfigureAwait(true); 

                    if (res == null) success = true;
                };
                if (!success) return null;
                GetSalesPersonPendingOrderQuery query = new GetSalesPersonPendingOrderQuery()
                {
                    CustomerId= request.CustomerId,
                    SalesPersonId= (request.SalesPersonId == null) ? _currentUser.UserId : request.SalesPersonId??Guid.Empty,
                    OrderId=request.OrderId
                };
                var result = await _commandBus.SendAsync(query);
                await AddPendingOrdersListAsync(request.orderItems.First(), cancellationToken);
                var draftOrder = await ApplyDiscounts(result,request.CustomerId, _currentUser.UserId,request.SupplierOrganizationId, cancellationToken);
                return draftOrder;
            }
            catch (Exception e)
            {
                _logger.Error( e.Message);
                _logger.Error( e.InnerException?.Message);
                throw e;
            }
          
        }
        private async Task AddPendingOrdersListAsync(OrderItemCreateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var pendingOrders = await _redisCache.GetAsync<List<PendingOrdersModel>>("pending_orders", cancellationToken);
                if (pendingOrders == null)
                {
                    await _redisCache.AddOrUpdateAsync<List<PendingOrdersModel>>("pending_orders",
                        new List<PendingOrdersModel>
                        {
                            new PendingOrdersModel
                            {
                                Id = request.OrderId,
                                CustomerId = request.CustomerId,
                                SalesPersonId = (request.SalesPersonId == null) ? _currentUser.UserId :request.SalesPersonId.Value
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
                            SalesPersonId = (request.SalesPersonId == null) ? _currentUser.UserId :request.SalesPersonId.Value
                        });
                        await _redisCache.AddOrUpdateAsync<List<PendingOrdersModel>>("pending_orders", pendingOrders,
                            cancellationToken);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
          
        }
        private async Task<CachedOrder> ApplyDiscounts(OrderDto result,Guid customerId,Guid salesPersonId, Guid supplierOrganizationId, CancellationToken cancellationToken)
        {
            try
            {
                #region Calculate discounts &  totals
                var draftOrder= _mapper.Map<CachedOrder>(result);
                var key = customerId.ToString() + salesPersonId + draftOrder.Id;
                draftOrder.OrderItems.ForEach(async item =>
                {
                    var discount= (await _commandBus.SendAsync(new GetActiveDiscountByProductQuery() { 
                            OrganizationId = supplierOrganizationId,
                            ProductId = item.ProductId
                        }
                    )).LastOrDefault(disc=>disc.ThresholdQuantity>=item.Quantity);
                    if (discount != null)
                    {
                    
                        item.Discount =(double) discount.DiscountRate;
                    }
                });
                draftOrder.OrderDiscount = draftOrder.OrderItems.CalculateDiscount();

                draftOrder.OrderTotal = draftOrder.OrderItems.CalculateTotalIncTax();
                await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
                #endregion
                return draftOrder;
            }
            catch (Exception e)
            {
                throw e;
            }
          
        }
       
    }
}
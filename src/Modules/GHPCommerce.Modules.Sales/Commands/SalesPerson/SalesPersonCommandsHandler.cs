using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders.Common;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Sales.Commands.SalesPerson.Commands;
using GHPCommerce.Modules.Sales.Hubs;
using GHPCommerce.Modules.Sales.Models;
using Microsoft.AspNetCore.SignalR;
using NLog;


namespace GHPCommerce.Modules.Sales.Commands.SalesPerson
{
    public class SalesPersonCommandsHandler :ICommandHandler<OrderItemCreateCommandV2, ValidationResult>
    {

        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;
        private readonly IHubContext<InventSumHub> _hubContext;
        private readonly Logger _logger;
        public SalesPersonCommandsHandler(
            IMapper mapper,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICurrentUser currentUser,
            ICache redisCache,
            IHubContext<InventSumHub> hubContext, Logger logger)
        {
            _mapper = mapper;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _redisCache = redisCache;
            _hubContext = hubContext;
            _logger = logger;
        }

        private InventSumCreatedEvent GetInventSums(string key)
        {
            var inventSums = _redisCache.Get<InventSumCreatedEvent>(key.ToLower());
            return inventSums;
        }

        private static CachedInventSum GetAvailableStockForReservation(IOrderItem request, CachedInventSumCollection collection)
        {
            return collection.CachedInventSums
                .Where(i =>
                        i.OrganizationId == request.SupplierOrganizationId 
                        && i.ProductId == request.ProductId 
                        && i.IsPublic
                        && i.InternalBatchNumber ==request.InternalBatchNumber //10 should be brought from i.BestBeforeDate
                )
                .OrderBy(i => i.ExpiryDate)
                .FirstOrDefault()?
                .ShallowClone();
        }

        

        public async Task<ValidationResult> Handle(OrderItemCreateCommandV2 request, CancellationToken cancellationToken)
        {
            
            var inventKey = request.ProductId.ToString() + request.SupplierOrganizationId;
            await LockProvider<string>.ProvideLockObject(inventKey).WaitAsync( cancellationToken);
           
            ValidationResult validations = default;
            var validator = new OrderItemCreateCommandV2Validator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
            {
                LockProvider<string>.ProvideLockObject(inventKey).Release();
                return validationErrors;
            }

           

            CachedOrder draftOrder;
            CachedOrderItem orderItem;

            string key = request.CustomerId.ToString() + _currentUser.UserId + request.OrderId;
            var lookupKey = request.CustomerId.ToString() + _currentUser.UserId;
            var inventSums = GetInventSums(inventKey);
         
            try
            {
              
                if (request.Quantity <= 0)
                    throw new InvalidOperationException("La ligne n'a pas pu être réservée");
                var quotaProduct = await _commandBus.SendAsync(new GetQuotaProductByIdQuery {  ProductId = request.ProductId}, cancellationToken);
                if (quotaProduct)
                {
                    var availableQnt = await _commandBus.SendAsync(new GetQuotasByProductQueryV3 { ProductId = request.ProductId }, cancellationToken); 
                    if (availableQnt <= 0 ||availableQnt < request.Quantity)
                        throw new InvalidOperationException($"Quota dépassé, quantité quota restante {availableQnt}");
                }

                
                var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (currentOrganizationId == null) throw new InvalidOperationException("resources not allowed, contact your admin");
                var psy = request.OrderType == OrderType.Psychotrope;
                var respected = request.ToBeRespected;
                await AddOrGetLookupsAsync(request.OrderId, lookupKey, cancellationToken);
               
                draftOrder = await GetCurrentPendingOrder(key, request.OrderId, request.SupplierOrganizationId,
                    docRef: request.DocumentRef, psychotropic: psy, toBeRespected: respected);
                
                if (draftOrder == null)
                    throw new InvalidOperationException("Commande non trouvée");
                if(draftOrder.OrderItems.Any(x=>x.ProductId == request.ProductId && x.InternalBatchNumber == request.InternalBatchNumber))
                    throw new InvalidOperationException("ça existe une ligne avec le même numéro de lot.");
                if (inventSums.CachedInventSumCollection == null)
                    throw new InvalidOperationException("Stock non disponible");
                draftOrder.IsSpecialOrder = request.SpecialOrder;
                var line = GetAvailableStockForReservation(request, inventSums.CachedInventSumCollection);
                if (line == null || request.Quantity > line.PhysicalAvailableQuantity)
                    throw new InvalidOperationException(
                        $"La ligne n'a pas pu être entièrement réservée, Quantité disponible = {line?.PhysicalAvailableQuantity}");
               
                var inventSumIndex = inventSums.CachedInventSumCollection.CachedInventSums
                    .FindIndex(x => x.ProductId == line.ProductId
                                    && x.InternalBatchNumber == line.InternalBatchNumber
                                    && x.OrganizationId == line.OrganizationId);
               

                var currentUser =await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },cancellationToken);
                if (string.IsNullOrEmpty(draftOrder.CreatedBy))
                {
                    draftOrder.CreatedBy = currentUser?.UserName;
                    draftOrder.CreatedDateTime = DateTimeOffset.Now;
                }

                orderItem = AddOrderItem(request, line);
                draftOrder.OrderItems.Add(orderItem);
                await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
                inventSums.CachedInventSumCollection.CachedInventSums[inventSumIndex].PhysicalReservedQuantity +=request.Quantity;
                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(inventKey, inventSums, cancellationToken);
                await _commandBus.SendAsync(
                   new UpdatePhysicalReservedQuantityCommand
                   {
                       ProductId = line.ProductId,
                       InternalBatchNumber = line.InternalBatchNumber,
                       Quantity = request.Quantity
                   }, cancellationToken);
                await AddPendingOrdersListAsync(request, cancellationToken);
                await DecreaseQuota(request, cancellationToken);
               

            }
            catch (Exception ex)
            {
                validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                _logger.Error( $"{nameof(SalesPersonCommandsHandler)} line 224" + ex.Message);
                _logger.Error(ex.InnerException?.Message);
            }
            finally
            {
                // release the lock
                LockProvider<string>.ProvideLockObject(inventKey).Release();
            }

            return validations;
        }

        private async Task DecreaseQuota(OrderItemCreateCommandV2 request, CancellationToken cancellationToken)
        {
            var vr = await _commandBus.SendAsync(new DecreaseQuotaCommand
                    { ProductId = request.ProductId, Quantity = request.Quantity, CustomerId = request.CustomerId },
                cancellationToken).ConfigureAwait(false);
            if (vr != null && !vr.IsValid)
            {
                foreach (var validationFailure in vr.Errors)
                    _logger.Error("Decrease quota errors: " + validationFailure.ErrorMessage);
            }
        }

        private async Task AddPendingOrdersListAsync(OrderItemCreateCommandV2 request, CancellationToken cancellationToken)
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
                            SalesPersonId = _currentUser.UserId
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
                        SalesPersonId = _currentUser.UserId
                    });
                    await _redisCache.AddOrUpdateAsync<List<PendingOrdersModel>>("pending_orders", pendingOrders,cancellationToken);
                }
            }
        }

        private async Task AddOrGetLookupsAsync( Guid orderId, string lookupKey,CancellationToken cancellationToken)
        {
            var lookUp = await _redisCache.GetAsync<List<Guid>>(lookupKey, cancellationToken);
            if (lookUp == null)
            {
                await _redisCache.AddOrUpdateAsync<List<Guid>>(lookupKey, new List<Guid> { orderId }, cancellationToken);
            }
            else if (lookUp.All(x => x != orderId))
            {
                lookUp.Add(orderId);
                await _redisCache.AddOrUpdateAsync<List<Guid>>(lookupKey, lookUp, cancellationToken);
            }
        }

        private CachedOrderItem AddOrderItem(OrderItemCreateCommandV2 request, CachedInventSum reservation)
        {
            CachedOrderItem orderItem = _mapper.Map<CachedOrderItem>(reservation);
            orderItem.Id = Guid.NewGuid();
            orderItem.Quantity = request.Quantity;
            orderItem.OrderId = request.OrderId;
            orderItem.InventSumId = reservation.Id;
            orderItem.ProductName = reservation.ProductFullName;
            orderItem.InternalBatchNumber = request.InternalBatchNumber;
            orderItem.ProductCode = request.ProductCode;
            orderItem.ZoneGroupId = request.ZoneGroupId;
            orderItem.ZoneGroupName = request.ZoneGroupName;
            orderItem.PickingZoneId = request.PickingZoneId;
            orderItem.PickingZoneName = request.PickingZoneName;
            orderItem.ExtraDiscount = request.ExtraDiscount / 100;
            orderItem.Packing = request.Packing;
            orderItem.Thermolabile = request.Thermolabile;
            orderItem.DefaultLocation = request.DefaultLocation;
            orderItem.PickingZoneOrder = request.PickingZoneOrder;
            orderItem.Tax = request.Tax;
            
            return orderItem;
            
        }

        private async  Task< CachedOrder> GetCurrentPendingOrder(string key, Guid orderId,  Guid? supplierOrganizationId = null, bool create = true, string docRef = "", bool? psychotropic = null, bool? toBeRespected = null)
        {
            //if create=false, we just return the existing Order
            var draftOrder =await  _redisCache.GetAsync<CachedOrder>(key);
            if (draftOrder == null)
            {
                if (!create) return null;
                if (supplierOrganizationId != null)
                {
                    draftOrder = new CachedOrder
                    {
                        Id = orderId,
                        SupplierId = supplierOrganizationId.Value,
                        CreatedByUserId = _currentUser.UserId,
                        OrderNumber = "BC-" + long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")),
                        OrderDate = DateTime.Now,
                        RefDocument = docRef

                    };
                }
            }

            if (draftOrder != null)
            {
                if (psychotropic.HasValue)
                    draftOrder.Psychotropic = psychotropic.Value;
                if (toBeRespected.HasValue)
                    draftOrder.ToBeRespected = toBeRespected.Value;
                draftOrder.OrderDiscount = draftOrder.OrderItems.CalculateTotalIncludeDiscount();
                draftOrder.OrderTotal = draftOrder.OrderItems.CalculateTotalIncTax();
            }
            return draftOrder;
        }
    }
}

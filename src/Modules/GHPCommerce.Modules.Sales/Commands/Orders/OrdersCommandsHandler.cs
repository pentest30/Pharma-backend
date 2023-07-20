using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Application.Catalog.Products.Queries;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders.Common;
using GHPCommerce.Core.Shared.Contracts.Organization.Queries;
using GHPCommerce.Core.Shared.Contracts.PickingZone.Queries;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Core.Shared.Contracts.ZoneGroup.Queries;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Core.Shared.PreparationOrder.Commands;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;
using GHPCommerce.Core.Shared.PreparationOrder.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.PreparationOrder.Commands;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Hubs;
using GHPCommerce.Modules.Sales.Models;
using GHPCommerce.Modules.Sales.Queries;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class OrdersCommandsHandler :
        ICommandHandler<UpdateOrderByPharmacistCommand, ValidationResult>,
        ICommandHandler<SendOrderByPharmacistCommand, ValidationResult>,
        ICommandHandler<CancelOrderByPharmacistCommand, ValidationResult>,
        ICommandHandler<OrderItemCreateCommand, ValidationResult>,
        ICommandHandler<OrderItemUpdateCommand, ValidationResult>,
        ICommandHandler<ChangeOrderStateCommand,ValidationResult>,
        ICommandHandler<OrderItemDeleteCommand, ValidationResult>,
        ICommandHandler<ChangePaymentStateCommand,ValidationResult>,
        ICommandHandler<OrderItemUpdateCommandV2, ValidationResult>,
        ICommandHandler<CancelPendingOrderCommand, ValidationResult>,
        ICommandHandler<ChangeExtraDiscountCommand, ValidationResult>,
        ICommandHandler<ChangeDiscountCommand, ValidationResult>,
        ICommandHandler<GeneratePreparationOrderCommand, ValidationResult>, 
        ICommandHandler<UpdateOrderDiscountCommandV2, ValidationResult>


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
        private readonly OpSettings _opSettings;
        public OrdersCommandsHandler(IOrdersRepository ordersRepository,
            IMapper mapper,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICurrentUser currentUser,
           // ILockProvider<string> lockProvider,
            ICache redisCache,
            IHubContext<InventSumHub> hubContext,
            Logger logger,
            MedIJKModel model,
            OpSettings opSettings)
        {
            _ordersRepository = ordersRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
           // LockProvider<string> = lockProvider;
            _redisCache = redisCache;
            _hubContext = hubContext;
            _logger = logger;
            _model = model;
            _opSettings = opSettings;
        }
        
        //Save final Order header
        public async Task<ValidationResult> Handle(UpdateOrderByPharmacistCommand request, CancellationToken cancellationToken)
        {
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();

            if (currentOrganizationId == null)
                throw new InvalidOperationException();
            string key = request.SupplierId.ToString() + _currentUser.UserId;
            var order = await GetCurrentPendingOrder(key, request.Id, cancellationToken, request.SupplierId, false);
            order.ExpectedShippingDate = request.ExpectedShippingDate;
            order.OrderDate = DateTimeOffset.Now.Date;
            order.CustomerId = request.CustomerId;
            order.RefDocument = request.RefDocument;
            var currentSupplier = await _commandBus.SendAsync(new GetWholesaleByIdQuery
            { CustomerOrganizationId = currentOrganizationId.Value, SupplierOrganizationId = request.SupplierId }, cancellationToken);
            //order.Comment=request.Comment Not yet added to entity
            //order.AttachedDocuments Uploads.. To Do
            //In orderItem we have to store MinExpiry date condition
            order.OrderDiscount =order.OrderItems. CalculateTotalIncludeDiscount();//Calculated=Line discounts +Global discount [Promotions & Packs]
            order.OrderTotal = order.OrderItems.CalculateTotalIncTax();
             await _redisCache.AddOrUpdateAsync<CachedOrder>(key, order, cancellationToken);
            string salesKey = order.Id.ToString() + currentSupplier.DefaultSalesPersonId;

             await _redisCache.AddOrUpdateAsync<CachedOrder>(salesKey, order, cancellationToken);

            return default;
        }
        
        //Send Order and it will be accessible by  salesperson

        public async Task<ValidationResult> Handle(SendOrderByPharmacistCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderType == OrderType.Psychotrope && (String.IsNullOrEmpty(request.RefDocument) || string.IsNullOrWhiteSpace(request.RefDocument)))
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Validation Error", "Ref document est obligatoire!")
                    }
                };
                
            }
            var existingOrder = await _ordersRepository.Table
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (existingOrder != null)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Missing order", "Bon de commande validé!")
                    }
                };
                
            }
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (currentOrganizationId == null)
                throw new InvalidOperationException();
            var currentUserId = request.DefaultSalesPerson ?? _currentUser.UserId;
            var key = request.CustomerId.ToString() + currentUserId;
            var cachedOrder = await GetCurrentPendingOrder(key + request.Id, request.Id, cancellationToken,
                request.SupplierId, false);
            if (cachedOrder == null)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Missing order", "Bon de commande non trouvé!")
                    }
                };
                
            }

            try
            {
                cachedOrder.OrderDiscount = cachedOrder.OrderItems.CalculateTotalIncludeDiscount();
                cachedOrder.OrderTotal = cachedOrder.OrderItems.CalculateTotalIncTax();
                await LockProvider<string>.ProvideLockObject(request.Id.ToString()).WaitAsync( cancellationToken);
                var lookUp = await _redisCache.GetAsync<List<Guid>>(key, cancellationToken);
                lookUp?.Remove(request.Id);
                await _redisCache.AddOrUpdateAsync<List<Guid>>(key, lookUp, cancellationToken);
                var customer = await GetCustomerAsync(request, cancellationToken);
                var order = await GetOrderAsync(request, customer, cachedOrder, cancellationToken);
                await CheckNullPickingZones( order, cancellationToken);
                _ordersRepository.Add(order);
                await _ordersRepository.UnitOfWork.SaveChangesAsync();
                // checks  If the TD validation is required for psychotropic orders. 
                if (!_opSettings.RequireTDValidation)
                {
                    await _commandBus.SendAsync(new GeneratePreparationOrderCommand { OrderId = order.Id },cancellationToken);
                }
                else if (_opSettings.RequireTDValidation && order.OrderType != OrderType.Psychotrope )
                {
                    await _commandBus.SendAsync(new GeneratePreparationOrderCommand { OrderId = order.Id },cancellationToken);
                }
                await _redisCache.ExpireAsync<CachedOrder>(key + request.Id, cancellationToken);
                await DeletePendingOrderByIdAsync(request.Id, cancellationToken);

            }
            catch (Exception e)
            {

                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
                if (e is AggregateException exception)
                {
                    var validationResult = new ValidationResult();
                    foreach (var msg in exception.InnerExceptions)
                    {
                        validationResult.Errors.Add(new ValidationFailure("Validation errors", msg.Message));
                    }

                    return validationResult;
                }

                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Missing order", e.Message)
                    }
                };

            }
            finally
            {
                LockProvider<string>.ProvideLockObject(request.Id.ToString()).Release();
            }

            return null;
        }

//  this bunch of logic should be moved from here to pending order creation region 
        private async Task CheckNullPickingZones( Order order, CancellationToken cancellationToken)
        {
            var xZone = await _commandBus.SendAsync(
                new GetPickingZoneByNameQuery { ZoneName = "X" }, cancellationToken);

            order.OrderItems.ForEach(
                async item =>
                {
                    if (string.IsNullOrEmpty(item.DefaultLocation) ||
                        string.IsNullOrEmpty(item.PickingZoneName)
                        ||
                        item.ZoneGroupId == null || item.ZoneGroupId == Guid.Empty)
                    {
                        var product = await _commandBus.SendAsync(new GetProductByIdQuery
                        {
                            Id = item.ProductId
                        }, cancellationToken);
                        if (product != null)
                        {
                            if (product.PickingZone == null)
                            {
                                product.PickingZone = new PickingZone
                                {
                                    Name = xZone.Name,
                                    Id = xZone.Id,
                                    ZoneGroup =
                                        new ZoneGroup(xZone.ZoneGroup.Id,
                                            xZone.ZoneGroup.Name, null, xZone.ZoneGroup.Order,
                                            xZone.ZoneGroup.Printer
                                        ),
                                    ZoneGroupId = xZone.ZoneGroupId,
                                    Order = xZone.Order
                                };
                            }

                            item.ZoneGroupId = product.PickingZone.ZoneGroupId;
                            item.ZoneGroupName = product.PickingZone.ZoneGroup.Name;
                            item.PickingZoneId = product.PickingZone.Id;
                            item.PickingZoneName = product.PickingZone.Name;
                            item.PickingZoneOrder = product.PickingZone.Order;
                            item.DefaultLocation = product.DefaultLocation ?? "X";
                        }
                    }
                });
        }

        private async Task<Order> GetOrderAsync(SendOrderByPharmacistCommand request,CustomerDtoV1 customer, CachedOrder cachedOrder,CancellationToken cancellationToken)
        {
            var order = _mapper.Map<Order>(cachedOrder);
            var currentSupplier = await _commandBus.SendAsync(new GetWholesaleByIdQuery
                {
                    CustomerOrganizationId = customer.OrganizationId,
                    SupplierOrganizationId = request.SupplierId
                },
                cancellationToken);
            var psy = cachedOrder.Psychotropic;
            order.RefDocument = request.RefDocument;
            order.OrderStatus = OrderStatus.Pending;
            order.OrderType = psy ? OrderType.Psychotrope : OrderType.NonPsychotrope;
            order.ExpectedShippingDate = request.ExpectedShippingDate;
            order.ToBeRespected = request.ToBeRespected;
            order.IsSpecialOrder = request.IsSpecialOrder;
            order.OrderBenefit = cachedOrder.OrderItems.CalculateOrderBenefit();
            order.OrderBenefitRate = cachedOrder.OrderItems.CalculateGlobalRateBenefit(order.OrderBenefit);
            order.SupplierName = currentSupplier.Name;
            order.CustomerName = customer.Name;
            order.OrderDate = DateTimeOffset.Now.Date;
            order.ExpectedShippingDate = request.ExpectedShippingDate;
            order.DefaultSalesPersonId = _currentUser.UserId;
            order.CustomerId = customer.OrganizationId;
            order.OrderStatus = OrderStatus.Pending;
            return order;
        }

        private async Task<CustomerDtoV1> GetCustomerAsync(SendOrderByPharmacistCommand request, CancellationToken cancellationToken)
        {
            CustomerDtoV1 customer;
            if ( request.CustomerId.HasValue)
                customer = await _commandBus.SendAsync(new GetCustomerByIdQuery { Id = request.CustomerId.Value },cancellationToken);
            else
            {
                var orgId = (await _currentOrganization.GetCurrentOrganizationIdAsync());
                customer = new CustomerDtoV1
                {
                    OrganizationId = orgId.Value,
                    Name = await _currentOrganization.GetCurrentOrganizationNameAsync()
                };
            }

            return customer;
        }

       

        public async Task<ValidationResult> Handle(CancelOrderByPharmacistCommand request, CancellationToken cancellationToken)
        {
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (currentOrganizationId == null)
                throw new InvalidOperationException();

            var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true }, cancellationToken);
            string key;
            if (currentUser.UserRoles.Any(x => x.Role.Name == "Admin"))
                key = request.SupplierId.ToString() + _currentUser.UserId;
            else key = request.Id.ToString() + _currentUser.UserId;
            var cachedOrder = await GetCurrentPendingOrder(key + request.Id, request.Id, cancellationToken, request.SupplierId, false);
            if (cachedOrder == null || cachedOrder.Id != request.Id)
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Missing order", "Vous n'avez pas sélectionné la bonne commande !")
                    }
                };
            cachedOrder.OrderItems?.ForEach(async i =>
            {
                string inventKey = i.ProductId.ToString() + cachedOrder.SupplierId;
                var invent = await _redisCache.GetAsync<InventSumCreatedEvent>(inventKey, cancellationToken);
                if (invent != null)
                {
                    var entry = invent.CachedInventSumCollection.CachedInventSums.FirstOrDefault(t =>
                        t.Id == i.InventSumId);
                    if (entry != null)
                    {
                        entry.PhysicalReservedQuantity -= i.Quantity;
                    }

                    await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(inventKey, invent, cancellationToken);
                }

            });
            await _redisCache.ExpireAsync<CachedOrder>(key, cancellationToken);

            return default;
        }
        static bool  locked = false;

        public async Task<ValidationResult> Handle(OrderItemCreateCommand request, CancellationToken cancellationToken)
        {
            var validator = new OrderItemCreateCommandValidator();

            ValidationResult result = default;
            var inventKey = request.ProductId.ToString() + request.SupplierOrganizationId;
            try
            {
                await LockProvider<string>.ProvideLockObject(inventKey).WaitAsync(cancellationToken);
                var validationErrors = await validator.ValidateAsync(request, cancellationToken);
                if (!validationErrors.IsValid)
                {
                    return validationErrors;
                }

                User currentUser;
                if (request.SalesPersonId.IsNullOrEmpty())
                    currentUser = await _commandBus.SendAsync(
                        new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true }, cancellationToken);
                else
                    currentUser = await _commandBus.SendAsync(
                        new GetUserQuery { Id = request.SalesPersonId.Value, IncludeRoles = true },
                        cancellationToken);

                var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                // si le client n'est pas enregistré dans le systeme comme officine on rejete la création de  la commande
                if (currentOrganizationId == null)
                    throw new InvalidOperationException("Resources not allowed. contact your admin please");
                string key;
                if (currentUser.UserRoles.Any(x =>
                        x.Role.Name == "SalesPerson" || x.Role.Name == "OnlineCustomer" ||
                        x.Role.Name == "Supervisor"))
                {
                    key = request.CustomerId.ToString() + currentUser.Id;
                    key += request.OrderId;
                }
                else key = request.SupplierOrganizationId.ToString() + _currentUser.UserId;

                var psy = request.OrderType == OrderType.Psychotrope;
                var respected = request.ToBeRespected;
                var draftOrder = await GetCurrentPendingOrder(key, request.OrderId, cancellationToken,
                        request.SupplierOrganizationId, docRef: request.DocumentRef, psychotropic: psy,
                        toBeRespected: respected, refDocumentHpcs: request.RefDocumentHpcs,
                        dateDocumentHpcs: request.DateDocumentHpcs, customerId: request.CustomerId)
                    .ConfigureAwait(true);
                var product =
                    await _commandBus.SendAsync(new GetProductById { Id = request.ProductId }, cancellationToken);
                if (!product.Psychotropic && psy || !psy && product.Psychotropic ||
                    product.ProductState == ProductState.Deactivated)
                {
                    throw  new InvalidOperationException("Produit désactivé");
                }

                if (string.IsNullOrEmpty(draftOrder.CreatedBy))
                {
                    draftOrder.CreatedBy = currentUser.UserName;
                    draftOrder.CreatedByUserId = currentUser.Id;
                    draftOrder.CreatedDateTime = DateTimeOffset.Now;
                }

                _initialInventSums = new InventSumCreatedEvent();
                var reserved = await AddOrUpdateOrderItems(request, draftOrder, inventKey, cancellationToken)
                    .ConfigureAwait(true);
                if (reserved == null || !reserved.Any())
                {
                    throw new InvalidOperationException(
                        "La création de la commande est impossible pour le moment,réessayer plus tard.");
                }

                foreach (var res in reserved)
                    await _commandBus.SendAsync(
                        new UpdatePhysicalReservedQuantityCommand
                        {
                            ProductId = res.ProductId,
                            InternalBatchNumber = res.InternalBatchNumber,
                            Quantity = Convert.ToInt32(Math.Round(res.PhysicalReservedQuantity, 0)),
                            OrganizationId = request.SupplierOrganizationId
                        }, cancellationToken);

                await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
            }
            catch (Exception ex)
            {
                result = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                // if (request.OldQuantity == 0 && ) _redisCache.Expire<CachedOrder>(key);
                _logger.Error(nameof(OrderItemCreateCommand) + ": " + ex.Message);
                _logger.Error(ex.InnerException?.Message);
                // ignored
            }
            finally
            {
                LockProvider<string>.ProvideLockObject(inventKey).Release();
            }

            return result;
        }

        private async Task<List<CachedInventSum>> AddOrUpdateOrderItems(IOrderItem request, CachedOrder draftOrder, string key, CancellationToken cancellationToken)
        {
            
            var reserved = await TryReserve(request, key, cancellationToken).ConfigureAwait(true);
            var lines = draftOrder.OrderItems.ShallowClone();
            for (int i = 0; i < reserved.Count; i++)
            {
                var inventSumReservationDto = reserved[i];
                CachedOrderItem orderItem = _mapper.Map<CachedOrderItem>(inventSumReservationDto);
                orderItem.Id = Guid.NewGuid();
                orderItem.OrderId = request.OrderId;
                orderItem.InventSumId = inventSumReservationDto.Id;
                orderItem.ProductName = inventSumReservationDto.ProductFullName;
                orderItem.InternalBatchNumber = inventSumReservationDto.InternalBatchNumber;
                orderItem.ProductCode = inventSumReservationDto.ProductCode;
                orderItem.PickingZoneId = request.PickingZoneId;
                orderItem.PickingZoneName = request.PickingZoneName;
                orderItem.ZoneGroupId = request.ZoneGroupId;
                orderItem.ZoneGroupName = request.ZoneGroupName;
                orderItem.Packing = inventSumReservationDto.Packing;
                orderItem.VendorBatchNumber = inventSumReservationDto.VendorBatchNumber;
                orderItem.ExtraDiscount = inventSumReservationDto.ExtraDiscount / 100;
                orderItem.DefaultLocation = request.DefaultLocation;
                orderItem.PickingZoneOrder = request.PickingZoneOrder;
                int qty = 0;
                int.TryParse( (Math.Round(reserved[i].PhysicalReservedQuantity,0)).ToString(CultureInfo.InvariantCulture),out qty);
                orderItem.Quantity = qty;
                int index = lines.FindIndex(o =>
                    o.InternalBatchNumber == orderItem.InternalBatchNumber &&
                    o.VendorBatchNumber == orderItem.VendorBatchNumber &&
                    o.ProductId == orderItem.ProductId &&
                    o.Color == orderItem.Color &&
                    o.Size == orderItem.Size);
                if (index < 0)
                    draftOrder.OrderItems.Add(orderItem);
                else
                    draftOrder.OrderItems[index].Quantity += orderItem.Quantity;

            }
          
            return reserved;
        }

        private async Task<List<CachedInventSum>> TryReserve(IOrderItem request, string key, CancellationToken cancellationToken)
        {
            try
            {
                int qty = request.Quantity;
                if (qty <= 0)
                    throw new InvalidOperationException("La ligne n'a pas pu être réservée");
                // onhand =2 , reserved =2
                var inventSums = await GetInventSums(key, cancellationToken);
                if(inventSums == null )
                    throw new InvalidOperationException("Stock non disponible");
                _initialInventSums = (InventSumCreatedEvent)inventSums.Clone();
                _initialInventSums.CachedInventSumCollection =
                    (CachedInventSumCollection)inventSums.CachedInventSumCollection.Clone();
                if (inventSums.CachedInventSumCollection == null)
                    throw new InvalidOperationException("Stock non disponible");
                var entries = GetAvailableStockForReservation(request, inventSums.CachedInventSumCollection);
                if (!entries.Any()) throw new InvalidOperationException("Stock non disponible");

               /* if (qty > entries.Sum(x => x.PhysicalAvailableQuantity))
                    throw new InvalidOperationException(
                        $"La ligne n'a pas pu être entièrement réservée, Quantité disponible = {entries.Sum(x => x.PhysicalAvailableQuantity) + request.OldQuantity}");*/
                List<CachedInventSum> result = new List<CachedInventSum>();
                _initialInventSums.CachedInventSumCollection.CachedInventSums = entries.Clone().ToList();
                var batches = entries.Where(i => string.IsNullOrEmpty(request.InternalBatchNumber)
                                                 || i.InternalBatchNumber == request.InternalBatchNumber &&
                                                 i.ProductId == request.ProductId ).ToList();
                for (int i = 0; i < batches.Count; i++)
                {
                    var inventSum = batches[i];
                    CachedInventSum reservation = (CachedInventSum)inventSum.Clone();
                    reservation.Error = false;

                    if (inventSum.PhysicalAvailableQuantity >= qty)
                    {
                        inventSum.PhysicalReservedQuantity += qty;
                        reservation.PhysicalReservedQuantity = qty;
                        result.Add(reservation);
                        break;
                    }

                    reservation.PhysicalReservedQuantity = (int)inventSum.PhysicalAvailableQuantity;
                    qty -= (int)reservation.PhysicalReservedQuantity >0 ? (int)reservation.PhysicalReservedQuantity : 0;

                    if (reservation.PhysicalReservedQuantity > 0)
                    {
                        result.Add(reservation);
                        inventSum.PhysicalReservedQuantity += reservation.PhysicalReservedQuantity;
                    }

                    batches[i] = inventSum;

                }

                inventSums.CachedInventSumCollection.CachedInventSums = entries;
                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSums.ShallowClone(), cancellationToken);

                return result;
            }
            catch (Exception e)
            {
                return  new List<CachedInventSum>();
            }
        }


        private static List<CachedInventSum> GetAvailableStockForReservation(IOrderItem request, CachedInventSumCollection collection)
        {
            return collection.CachedInventSums
                .Where(i =>
                        i.OrganizationId == request.SupplierOrganizationId &&
                        i.ProductId == request.ProductId &&
                        (request.MinExpiryDate == null ||
                         i.ExpiryDate >= request.MinExpiryDate) &&
                        i.IsPublic &&
                        i.Color == request.Color &&
                        i.Size == request.Size &&
                        (i.ExpiryDate == null ||
                         i.ExpiryDate.Value.Date.AddDays(10) > DateTime.Today) && i.PhysicalAvailableQuantity>0 //10 should be brought from i.BestBeforeDate
                )
                .OrderBy(i => i.ExpiryDate)
                .ToList()
                .Clone()
                .ToList();
        }

        private async Task<InventSumCreatedEvent> GetInventSums(string key, CancellationToken cancellationToken)
        {
            var inventSums =  await _redisCache.GetAsync<InventSumCreatedEvent>(key.ToLower(), cancellationToken);
            return inventSums?? new InventSumCreatedEvent();
        }

        private async Task<CachedOrder> GetCurrentPendingOrder(string key, Guid orderId,CancellationToken cancellationToken, Guid? supplierOrganizationId = null, bool create = true,
            string docRef = "", bool? psychotropic = null, bool? toBeRespected=null, string refDocumentHpcs = null,
            DateTime? dateDocumentHpcs = null, Guid? customerId = null)
        {
            //if create=false, we just return the existing Order
            var draftOrder = await _redisCache.GetAsync<CachedOrder>(key, cancellationToken);
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
                        RefDocument = docRef,
                        RefDocumentHpcs=refDocumentHpcs,
                        DateDocumentHpcs=dateDocumentHpcs    ,
                        CustomerId=customerId

                    };
                    if (psychotropic.HasValue)
                        draftOrder.Psychotropic = psychotropic.Value;
                    if (toBeRespected.HasValue)
                        draftOrder.ToBeRespected = toBeRespected.Value;
                    await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
                }
            }
            else
            {
                draftOrder.OrderDiscount = draftOrder.OrderItems.CalculateTotalIncludeDiscount();
                draftOrder.OrderTotal = draftOrder.OrderItems.CalculateTotalIncTax();
                
            }

            return draftOrder;
        }


        public async Task<ValidationResult> Handle(OrderItemUpdateCommand request, CancellationToken cancellationToken)
        {
            await LockProvider<string>.WaitAsync(request.SupplierOrganizationId + "/" + request.ProductId, cancellationToken);
            ValidationResult result = default;
            // on hand 5 , reserved 5 , old 3 , new 2
#region checks
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            // si le client n'est pas enregistré dans le systeme comme officine on rejete la création de  la commande
            if (currentOrganizationId == null) throw new InvalidOperationException();
            if (request.SupplierOrganizationId != Guid.Empty)
            {
                var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                        cancellationToken);

                string key;
                if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesPerson" || x.Role.Name == "Supervisor"))
                    key = request.OrderId.ToString() + _currentUser.UserId;
                else key = request.SupplierOrganizationId.ToString() + _currentUser.UserId;
                var draftOrder = await GetCurrentPendingOrder(key, request.OrderId, cancellationToken,
                    supplierOrganizationId: request.SupplierOrganizationId);
                if (draftOrder == null)
                    return new ValidationResult
                    {
                        Errors =
                        {
                            new ValidationFailure("Missed pending order", "Commande introuvable Ou non modifiable")
                        }
                    };

#endregion

                try
                {
                    var lines = draftOrder.OrderItems.Where(p => p.ProductId == request.ProductId)
                        .OrderByDescending(l => l.ExpiryDate).ToList();
                    var oldQty = lines.Sum(l => l.Quantity);
                    request.Quantity -= oldQty;

#region reserve the additional Quantity

                    if (request.Quantity > 0)
                    {
                        LockProvider<string>.Release(draftOrder.SupplierId.ToString() + request.ProductId);
                        return await _commandBus.SendAsync(
                            new OrderItemCreateCommand
                            {
                                OldQuantity = oldQty, Quantity = request.Quantity, Color = request.Color,
                                MinExpiryDate = request.MinExpiryDate, OrderId = request.OrderId,
                                SupplierOrganizationId = request.SupplierOrganizationId, ProductId = request.ProductId,
                                Size = request.Size
                            }, cancellationToken);
                    }

#endregion

                    if (request.Quantity == 0) throw new Exception("Quantity already reserved");

                    List<InventSumReservationDto> canceledReservations = new List<InventSumReservationDto>();
                    bool canceled = false;
                    try
                    {
                        if (request.Quantity < 0)
                        {
                            int qty = request.Quantity;


                            foreach (var line in lines)
                            {
                                var reservation = _mapper.Map<InventSumReservationDto>(line);
                                reservation.OrganizationId = draftOrder.SupplierId;
                                if (line.Quantity + qty < 0)
                                {
                                    canceledReservations.Add(reservation);
                                    qty += line.Quantity;
                                    line.Quantity = 0;
                                    continue;
                                }

                                reservation.Quantity = -qty;
                                line.Quantity += qty;
                                canceledReservations.Add(reservation);
                                break;
                            }

                            var resp = await _commandBus.SendAsync(
                                new CancelReservationsForB2BCustomerCommand { Reservations = canceledReservations },
                                cancellationToken);
                            canceled = (resp?.Errors == null || resp?.Errors.Count == 0);

                            if (canceled)
                            {
                                if (lines.All(p => p.Quantity == 0))
                                {
                                    draftOrder.OrderItems.Clear();
                                    await _redisCache.ExpireAsync<CachedOrder>(key, cancellationToken);
                                }
                                else
                                {
                                    draftOrder.OrderItems = lines.Where(p => p.Quantity > 0).ToList();
                                }

                                _redisCache.AddOrUpdate<CachedOrder>(key, draftOrder);

                            }
                            else
                            {
                                throw new Exception();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message);
                        _logger.Error(ex.InnerException?.Message);
                        if (canceled)
                        {
                            canceledReservations.ForEach(c => c.Quantity *= -1);
                            await _commandBus.SendAsync(
                                new CancelReservationsForB2BCustomerCommand { Reservations = canceledReservations },
                                cancellationToken);
                        }

                        result = new ValidationResult
                        {
                            Errors = { new ValidationFailure("Transaction rolled back", "La transaction a échoué") }
                        };
                    }
                }

#region Unlock

                catch (Exception ex)
                {
                    if (result != default)
                        result.Errors.Add(new ValidationFailure("Reservation failed", ex.Message));
                    else
                        result = new ValidationResult
                            { Errors = { new ValidationFailure("Reservation failed", ex.Message) } };
                    _logger.Error(ex.Message);
                    _logger.Error(ex.InnerException?.Message);

                    // ignored
                }
                finally
                {
                    // release the lock
                    LockProvider<string>.Release(draftOrder.SupplierId.ToString() + request.ProductId);

                }
            }

#endregion
            return result;
        }
        public async Task<ValidationResult> Handle(OrderItemDeleteCommand request, CancellationToken cancellationToken)
        {
            return await _commandBus.SendAsync(new OrderItemUpdateCommand { Quantity = 0, OrderId = request.OrderId, Color = request.Color, Size = request.Size, ProductId = request.ProductId,SupplierOrganizationId= request.SupplierId }, cancellationToken);
        }

        private async Task<ValidationResult> SaveOrderAsync(CachedOrder cachedOrder,string key, SendOrderByPharmacistCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = default;
            try 
            {
                Dictionary<Guid, double> dictionary = new Dictionary<Guid, double>();
                foreach (var item in cachedOrder.OrderItems)
                {
                    dictionary.Add(item.InventSumId, item.Quantity);
                    _= await _redisCache.GetAsync<InventSumCreatedEvent>(item.ProductId + request.SupplierId.ToString(), cancellationToken);
                }
                result = await _commandBus.SendAsync(new ReserveInventoryCommand { Reservations = dictionary }, cancellationToken);
                await _redisCache.ExpireAsync<CachedOrder>(key, cancellationToken);
               
            }
            catch (Exception ex)
            {
                if (result != null && (result.Errors == null || result.Errors.Count == 0))
                {
                    Dictionary<Guid, double> dictionary = new Dictionary<Guid, double>();
                    foreach (var item in cachedOrder.OrderItems)
                        dictionary.Add(item.InventSumId, -item.Quantity);
                    result = await _commandBus.SendAsync(new ReserveInventoryCommand { Reservations = dictionary }, cancellationToken);
                }
                result?.Errors.Add(new ValidationFailure("test", ex.Message));
                _logger.Error( ex.Message);
                _logger.Error( ex.InnerException?.Message);
            }
            return result;
        }

        public async Task<ValidationResult> Handle(ChangeOrderStateCommand request, CancellationToken cancellationToken)
        {
            string key = request.Id.ToString() + _currentUser.UserId;
            var persistedOrder = await _ordersRepository.Table.Where(x => x.Id == request.Id).Include(x => x.OrderItems).FirstOrDefaultAsync(cancellationToken);
            persistedOrder.OrderStatus = request.orderStatus;

            ValidationResult result = default;
            switch (request.orderStatus)
            {
                case OrderStatus.Accepted:
                {
                        break;

                }
                case OrderStatus.Processing:
                {
                        persistedOrder.ProcessedBy = _currentUser.UserId;
                        persistedOrder.ProcessingTime = DateTimeOffset.Now.Date;
                        break;

                }
                case OrderStatus.Shipping:
                {
                        // persist cache order
                        persistedOrder.ShippedBy = _currentUser.UserId;
                        persistedOrder.ShippingTime = DateTimeOffset.Now.Date;
                       // result = await PersistStockAsync(request.Id, request.SupplierId, cancellationToken);
                    break;

                }
                case OrderStatus.Completed:
                {
                        persistedOrder.CompletedBy = _currentUser.UserId;
                        persistedOrder.CompletedTime = DateTimeOffset.Now.Date;
                        break;
                }
                case OrderStatus.Canceled:
                {
                        persistedOrder.CanceledBy = _currentUser.UserId;
                        persistedOrder.CanceledTime = DateTimeOffset.Now.Date;
                        persistedOrder.CancellationReason = request.CancellationReason;
                        break;
                }
                case OrderStatus.Rejected:
                {
                        persistedOrder.RejectedBy = _currentUser.UserId;
                        persistedOrder.RejectedTime = DateTimeOffset.Now.Date;
                        persistedOrder.RejectedReason = request.RejectedReason;
                        break;
                }
            }
            _ordersRepository.Update(persistedOrder);
            try
            {
                await _ordersRepository.UnitOfWork.SaveChangesAsync();
                if(request.orderStatus == OrderStatus.Rejected || request.orderStatus == OrderStatus.Canceled)
                    await _redisCache.ExpireAsync<CachedOrder>(key, cancellationToken);

            }
            catch (Exception ex)
            {
                _logger.Error( ex.Message);
                _logger.Error( ex.InnerException?.Message);
            }
            return result;
        }
        public async Task<ValidationResult> Handle(ChangePaymentStateCommand request, CancellationToken cancellationToken)
        {
            var persistedOrder = await _ordersRepository.Table.Where(x => x.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
            persistedOrder.PaymentStatus = request.PaymentStatus;
            ValidationResult result = default;
            _ordersRepository.Update(persistedOrder);
            try
            {
                await _ordersRepository.UnitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.Error( ex.Message);
                _logger.Error( ex.InnerException?.Message);
            }
            return result;
        }
       


        public async Task<ValidationResult> Handle(OrderItemUpdateCommandV2 request, CancellationToken cancellationToken)
        {
            ValidationResult validations = default;
            var inventKey = request.ProductId.ToString() + request.SupplierOrganizationId;
            await LockProvider<string>.ProvideLockObject(inventKey).WaitAsync( cancellationToken);
            var userId = request.CreatedByUserId == Guid.Empty || request.CreatedByUserId == null
                ? _currentUser.UserId
                : request.CreatedByUserId;
            string key = request.CustomerId.ToString() + userId + request.OrderId;
            var inventSums = await GetInventSums(inventKey, cancellationToken);
            CachedOrderItem item = default;
            CachedOrder draftOrder = default;
            try
            {
                if (inventSums.CachedInventSumCollection == null)
                    throw new InvalidOperationException("Stock non disponible");
                var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (currentOrganizationId == null) throw new InvalidOperationException();
                draftOrder = await GetCurrentPendingOrder(key, request.OrderId, cancellationToken, request.SupplierOrganizationId).ConfigureAwait(true);
                if (draftOrder == null)
                    throw new InvalidOperationException("Commande non trouvée");
                var index = draftOrder.OrderItems
                    .FindIndex(x =>
                    x.InternalBatchNumber == request.InternalBatchNumber && x.ProductId == request.ProductId);
                if (index < 0)
                    throw new InvalidOperationException("Ligne commande non trouvée");

                item = draftOrder.OrderItems[index];
                var line = GetAvailableSingleStockForReservation(request, inventSums.CachedInventSumCollection);
                if (line == null) throw new InvalidOperationException("Stock non disponible");
                var availableQnt = line.PhysicalAvailableQuantity>=0 ? line.PhysicalAvailableQuantity : 0;
                if (request.Quantity > availableQnt + item.Quantity )
                    throw new InvalidOperationException(
                        $"La ligne n'a pas pu être entièrement réservée, Quantité disponible = {availableQnt}");
                var inventSumIndex = inventSums.CachedInventSumCollection.CachedInventSums
                    .FindIndex(x =>
                        x.ProductId == line.ProductId && x.ExpiryDate == line.ExpiryDate &&
                        x.InternalBatchNumber == line.InternalBatchNumber 
                        && x.OrganizationId == line.OrganizationId);
                int? oldQnt = item.Quantity;
                int? qnt = request.Quantity;
                var newQnt = 0;
                if ( qnt.Value> 0)
                {
                    newQnt = -oldQnt.Value + qnt.Value;
                    inventSums.CachedInventSumCollection.CachedInventSums[inventSumIndex].PhysicalReservedQuantity +=newQnt;
                    if (inventSums.CachedInventSumCollection.CachedInventSums[inventSumIndex].PhysicalReservedQuantity < 0)
                    {
                        inventSums.CachedInventSumCollection.CachedInventSums[inventSumIndex].PhysicalReservedQuantity = 0;
                    }

                    item.Quantity = request.Quantity;
                    draftOrder.OrderItems[index] = item;
                    if (newQnt > 0) await DecreaseQuota(request, newQnt, cancellationToken);
                    else await IncreaseQuota(request, Math.Abs(newQnt), cancellationToken);

                }
                else if ( qnt.Value <0)
                {
                    if (inventSums.CachedInventSumCollection.CachedInventSums[inventSumIndex].PhysicalReservedQuantity +
                        qnt.Value >= 0)
                        inventSums.CachedInventSumCollection.CachedInventSums[inventSumIndex]
                            .PhysicalReservedQuantity += qnt.Value;
                    else
                        inventSums.CachedInventSumCollection.CachedInventSums[inventSumIndex].PhysicalReservedQuantity =0;
                    newQnt = qnt.Value;
                    draftOrder.OrderItems.Remove(item);
                    await IncreaseQuota(request, oldQnt.Value , cancellationToken);
                    
                }
                else
                    throw new InvalidOperationException("La valeur de la quantité réservée ne peut pas être négative ");

                var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = false },cancellationToken);
                if (!string.IsNullOrEmpty(draftOrder.CreatedBy))
                {
                    draftOrder.UpdatedBy = currentUser?.UserName;
                    draftOrder.UpdatedDateTime = DateTimeOffset.Now;
                }
                await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(inventKey, inventSums, cancellationToken);
                await UpdateInventDb(line,newQnt, cancellationToken);
              
            }
            catch (Exception ex)
            {
                validations = new ValidationResult{ Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                _logger.Error( $"{nameof(OrdersCommandsHandler)} line 224" + ex.Message);
                _logger.Error(ex.InnerException?.Message);
            }
            finally
            {
                // release the lock
                LockProvider<string>.ProvideLockObject(inventKey).Release();
                //_throttler.Release();
            }

            return validations;
        }

        private async Task UpdateInventDb(CachedInventSum line, int qnt, CancellationToken cancellationToken)
        {
            await _commandBus.SendAsync(
                new UpdatePhysicalReservedQuantityCommand
                {
                    ProductId = line.ProductId,
                    InternalBatchNumber = line.InternalBatchNumber,
                    Quantity = qnt
                }, cancellationToken);
        }

        private static CachedInventSum GetAvailableSingleStockForReservation(IOrderItem request, CachedInventSumCollection collection)
        {
            return collection.CachedInventSums
                .Where(i =>
                        i.OrganizationId == request.SupplierOrganizationId &&
                        i.ProductId == request.ProductId &&
                        (request.MinExpiryDate == null ||
                         i.ExpiryDate >= request.MinExpiryDate) &&
                        i.IsPublic &&
                        i.Color == request.Color &&
                        i.Size == request.Size &&
                        (i.ExpiryDate == null ||
                         i.ExpiryDate.Value.Date.AddDays(10) > DateTime.Today)
                        && i.InternalBatchNumber ==
                        request.InternalBatchNumber //10 should be brought from i.BestBeforeDate
                )
                .OrderBy(i => i.ExpiryDate)
                .FirstOrDefault()?
                .ShallowClone();
        }
        private async Task DecreaseQuota(OrderItemUpdateCommandV2 request,int qnt, CancellationToken cancellationToken)
        {
            var currentUser = request.CreatedByUserId ?? _currentUser.UserId;
            var vr = await _commandBus.SendAsync(new DecreaseQuotaCommand
                {
                    ProductId = request.ProductId, Quantity = qnt, CustomerId = request.CustomerId, SalesPersonId =currentUser
                },
                cancellationToken);
            if (vr != null && !vr.IsValid)
            {
                foreach (var validationFailure in vr.Errors)
                    _logger.Error("Decrease quota errors: " + validationFailure.ErrorMessage);
            }
        }

        private async Task IncreaseQuota(OrderItemUpdateCommandV2 request, int qnt, CancellationToken cancellationToken)
        {
            var currentUser = request.CreatedByUserId ?? _currentUser.UserId;

            var vr = await _commandBus.SendAsync(new IncreaseQuotaCommand
            {
                ProductId = request.ProductId,
                Quantity = qnt,
                CustomerId = request.CustomerId,
                SalesPersonId =currentUser
            },
                cancellationToken);
            if (vr != null && !vr.IsValid)
            {
                foreach (var validationFailure in vr.Errors)
                    _logger.Error("Increase quota errors: " + validationFailure.ErrorMessage);
            }
        }
       

        private async Task SendNotification( dynamic obj,CancellationToken cancellationToken)
        {
            await _hubContext.Clients.All.SendAsync("productQuantityChanged",new {obj.productId, obj.quantity }, cancellationToken);
        }

        public async Task<ValidationResult> Handle(CancelPendingOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (!request.Order.OrderItems.Any())
                {
                    await DeletePendingOrders(request, cancellationToken);
                    return default;
                }
                var order = _mapper.Map<Order>(request.Order);
                var customer = await _commandBus.SendAsync(new GetCustomerByIdQuery { Id = request.Order.CustomerId.Value }, cancellationToken);
                var currentSupplier = await _commandBus.SendAsync(new GetWholesaleByIdQuery
                    { CustomerOrganizationId = customer.OrganizationId, SupplierOrganizationId = request.Order.SupplierId.Value }, cancellationToken);
                order.CustomerName = customer.Name;
                order.SupplierName = currentSupplier.Name;
                order.DefaultSalesPersonId = _currentUser.UserId;
                order.OrderStatus = OrderStatus.Canceled;
                foreach (var orderOrderItem in order.OrderItems)
                    orderOrderItem.Quantity = -1 * orderOrderItem.Quantity;
                await _commandBus.SendAsync(new CancelPreparationsForOrderCommand { OrderId = order.Id });
                _ordersRepository.Add(order);
                await _ordersRepository.UnitOfWork.SaveChangesAsync();
                await DeletePendingOrders(request, cancellationToken);
            }
            catch (Exception e)
            {
                var key = request.Order.CustomerId.ToString() + _currentUser.UserId;
                var lookUp = await _redisCache.GetAsync<List<Guid>>(key, cancellationToken);
                lookUp?.Remove(request.Order.Id);
                await _redisCache.ExpireAsync<CachedOrder>(key + request.Order.Id, cancellationToken);
                if (lookUp != null && lookUp.Any())
                     await _redisCache.AddOrUpdateAsync<List<Guid>>(key, lookUp, cancellationToken);
                else await _redisCache.ExpireAsync<List<Guid>>(key, cancellationToken).ConfigureAwait(true);
                _logger.Error( e.Message);
                _logger.Error( e.InnerException?.Message);
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Removing order error", e.Message)
                    }
                };
             
            }

            return default;

        }

        private async Task DeletePendingOrders(CancelPendingOrderCommand request, CancellationToken cancellationToken)
        {
            var userId = request.Order.CreatedByUserId == Guid.Empty ?_currentUser.UserId : request.Order.CreatedByUserId;
            var key = request.Order.CustomerId.ToString() + userId;
            var lookUp = await _redisCache.GetAsync<List<Guid>>(key, cancellationToken);
            lookUp?.Remove(request.Order.Id);
            await _redisCache.ExpireAsync<CachedOrder>(key + request.Order.Id, cancellationToken).ConfigureAwait(true);
            if (lookUp != null && lookUp.Any())
                await _redisCache.AddOrUpdateAsync<List<Guid>>(key, lookUp, cancellationToken);
            else await _redisCache.ExpireAsync<List<Guid>>(key, cancellationToken).ConfigureAwait(true);
            await DeletePendingOrderByIdAsync(request.Order.Id, cancellationToken);
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

        public async Task<ValidationResult> Handle(ChangeExtraDiscountCommand request, CancellationToken cancellationToken)
        {
            string key = request.CustomerId.ToString() + _currentUser.UserId + request.Id;
            await LockProvider<string>.ProvideLockObject(key).WaitAsync(cancellationToken);
            try
            {
                
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if(orgId == Guid.Empty)
                    throw new InvalidComObjectException("Organization Id should not be null or empty");
                var draftOrder = await GetCurrentPendingOrder(key, request.Id, cancellationToken,
                    orgId);
                if (draftOrder == null)
                    throw new InvalidOperationException("Commande non trouvée");
                var index = draftOrder.OrderItems.FindIndex(x => x.InternalBatchNumber == request.InternalBatchNumber && x.ProductId == request.ProductId);
                if (index < 0)
                    throw new InvalidOperationException("Ligne commande non trouvée");

                var item = draftOrder.OrderItems[index];
                item.ExtraDiscount = Math.Round(request.Discount, 4);
                draftOrder.OrderItems[index] = item;
                 await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
                LockProvider<string>.ProvideLockObject(key).Release();

            }
            catch (Exception e)
            {
                LockProvider<string>.ProvideLockObject(key).Release();
                _logger.Error( e.Message);
                _logger.Error( e.InnerException?.Message);
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Removing order error", e.Message)
                    }
                };
            }

            return default;
        }
        public async Task<ValidationResult> Handle(ChangeDiscountCommand request, CancellationToken cancellationToken)
        {
            string key = request.CustomerId.ToString() + _currentUser.UserId + request.Id;
            await LockProvider<string>.ProvideLockObject(key).WaitAsync(cancellationToken);
            Console.WriteLine($"{key} entred ..");
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == Guid.Empty)
                    throw new InvalidComObjectException("Organization Id should not be null or empty");
                var draftOrder = await GetCurrentPendingOrder(key, request.Id, cancellationToken,
                    orgId).ConfigureAwait(true);
                if (draftOrder == null)
                    throw new InvalidOperationException("Commande non trouvée");
                var index = draftOrder.OrderItems.FindIndex(x => x.InternalBatchNumber == request.InternalBatchNumber && x.ProductId == request.ProductId);
                if (index < 0)
                    throw new InvalidOperationException("Ligne commande non trouvée");

                var item = draftOrder.OrderItems[index];
                item.Discount = Math.Round(request.Discount, 4);
                draftOrder.OrderItems[index] = item;
                await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
                LockProvider<string>.ProvideLockObject(key).Release();
                Console.WriteLine($"{key} exited ..");

            }
            catch (Exception e)
            {
                _logger.Error( e.Message);
                _logger.Error( e.InnerException?.Message);
                LockProvider<string>.ProvideLockObject(key).Release();
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Removing order error", e.Message)
                    }
                };
            }

            return default;
        }

        public async Task<ValidationResult> Handle(GeneratePreparationOrderCommand request,CancellationToken cancellationToken)
        {
            var order = await _ordersRepository.Table.Where(x => x.Id == request.OrderId)
                .Include(x => x.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
            var XZone = await _commandBus.SendAsync(
                new GetPickingZoneByNameQuery { ZoneName = "X" }, cancellationToken);

            order.OrderItems.ForEach(
                async item =>
                {
                    if (string.IsNullOrEmpty(item.DefaultLocation) ||
                    string.IsNullOrEmpty(item.PickingZoneName)
                    ||
                    item.ZoneGroupId == null || item.ZoneGroupId == Guid.Empty)
                    {
                        var product = await _commandBus.SendAsync(new GetProductByIdQuery
                        {
                            Id = item.ProductId
                        });
                        if (product != null)
                        {
                            if (product.PickingZone == null)
                            {
                                product.PickingZone = new PickingZone
                                {
                                    Name = XZone.Name,
                                    Id = XZone.Id,
                                    ZoneGroup =
                                    new ZoneGroup(XZone.ZoneGroup.Id,
                                    XZone.ZoneGroup.Name, null, XZone.ZoneGroup.Order, XZone.ZoneGroup.Printer
                                    ),
                                    ZoneGroupId = XZone.ZoneGroupId,
                                    Order = XZone.Order
                                };
                            }
                            item.ZoneGroupId = product.PickingZone.ZoneGroupId;
                            item.ZoneGroupName = product.PickingZone.ZoneGroup.Name;

                            item.PickingZoneId = product.PickingZone.Id;
                            item.PickingZoneName = product.PickingZone.Name;
                            item.PickingZoneOrder = product.PickingZone.Order;
                            item.DefaultLocation = product.DefaultLocation??"X";

                        }

                    }
                });
            var HasAlreadyOps= (await _commandBus.SendAsync(new GetPOsByOrderQuery {
                OrderId=request.OrderId
            })).Count>0;
            if (order!=null  &&( order.OrderStatus == OrderStatus.Pending || order.OrderStatus == OrderStatus.Accepted
                || (order.OrderStatus==OrderStatus.Prepared&& !HasAlreadyOps)))
            {

                var customer = await _commandBus.SendAsync(new GetCustomerByOrganizationIdQuery { OrganizationId = order.CustomerId }, cancellationToken);
                if (order.OrderType == OrderType.Psychotrope)
                {
                    var psychotropicZone = await _commandBus.SendAsync(
                        new GetPickingZoneByTypeQuery { ZoneType = ZoneType.Psychtropic },
                        cancellationToken);
                    var psyCommand = new CreatePreparationOrderCommand();
                    psyCommand.ZoneGroupId = psychotropicZone.ZoneGroupId;
                    psyCommand.CodeAx = order.CodeAx;
                    psyCommand.ZoneGroupName = psychotropicZone.ZoneGroup.Name;
                    psyCommand.zoneGroupOrder = psychotropicZone.ZoneGroup.Order;
                    psyCommand.CustomerId = order.CustomerId;
                    psyCommand.OrderId = order.Id;
                    psyCommand.TotalZoneCount = 1;
                    psyCommand.TotalPackage = order.OrderItems.Where(c => c.Packing != 0).Sum(c => c.Quantity / c.Packing);
                    psyCommand.CustomerName = order.CustomerName;
                    psyCommand.OrderIdentifier = order.OrderNumberSequence.ToString();
                    psyCommand.OrderDate = order.OrderDate;
                    psyCommand.SectorName = customer.Sector;
                    psyCommand.SectorCode = customer.SectorCode;
                    psyCommand.PreparationOrderItems = _mapper.Map<List<PreparationOrderItemDtoV1>>(order.OrderItems);
                    await _commandBus.SendAsync(psyCommand, cancellationToken);
                }
                else
                {
                    var fridgeZone = await _commandBus.SendAsync(new GetPickingZoneByTypeQuery
                    { ZoneType = ZoneType.Fridge }, cancellationToken);
                    var ExpensiveSensitiveZone = await _commandBus.SendAsync(new GetPickingZoneByTypeQuery
                    { ZoneType = ZoneType.ExpensiveSensitive }, cancellationToken);
                    var itemsOrigin = order.OrderItems
                        .Where(c => (c.Packing != 0 && c.Quantity % c.Packing == 0
                        && c.PickingZoneId != fridgeZone.Id && c.PickingZoneId != ExpensiveSensitiveZone.Id))
                        .ToList();
                    var itemsBulk = order.OrderItems.Where(c => (c.PickingZoneId == fridgeZone.Id ||
                    c.Packing == 0 || (c.Packing != 0 && c.Quantity % c.Packing != 0) ||
                    c.PickingZoneId == ExpensiveSensitiveZone.Id)).ToList();
                    var totalPacking = 0;

                    var originZone = await _commandBus.SendAsync(new GetPickingZoneByTypeQuery { ZoneType = ZoneType.Origin }, cancellationToken);
                    try
                    {
                        foreach (var item in itemsBulk)
                        {
                            var itemClone = item.ShallowClone();
                            var restQnt = (item.Packing == 0 || item.PickingZoneId == fridgeZone.Id || item.PickingZoneId == ExpensiveSensitiveZone.Id) ? item.Quantity : item.Quantity % item.Packing;
                            var packing = (item.Packing == 0 || item.PickingZoneId == fridgeZone.Id || item.PickingZoneId == ExpensiveSensitiveZone.Id) ? 0 : item.Quantity / item.Packing;

                            totalPacking += packing;
                            item.Quantity = restQnt;
                            if (packing > 0)
                            {
                                itemClone.Quantity = packing * item.Packing;
                                itemClone.Packing = item.Packing;
                                itemClone.PickingZoneId = originZone.Id;
                                itemClone.PickingZoneName = originZone.Name;
                                itemClone.PickingZoneOrder = originZone.Order;
                                itemClone.ZoneGroupName = originZone.ZoneGroup.Name;
                                itemClone.ZoneGroupId = originZone.ZoneGroupId;
                                itemsOrigin.Add(itemClone);
                            }
                        }

                        foreach (var item in itemsOrigin)
                        {
                            item.PickingZoneId = originZone.Id;
                            item.PickingZoneName = originZone.Name;
                            item.ZoneGroupName = originZone.ZoneGroup.Name;
                            item.ZoneGroupId = originZone.ZoneGroupId;
                            item.PickingZoneOrder = originZone.Order;
                        }

                        if (itemsOrigin.Any())
                        {
                            // PR colis d'origine
                            var originPackingCommand = new CreatePreparationOrderCommand();
                            originPackingCommand.ZoneGroupId = originZone.ZoneGroupId;
                            originPackingCommand.ZoneGroupName = originZone.ZoneGroup.Name;
                            originPackingCommand.zoneGroupOrder = originZone.ZoneGroup.Order;
                            originPackingCommand.OrderNumberSequence = order.OrderNumberSequence;
                            originPackingCommand.CustomerId = order.CustomerId;
                            originPackingCommand.OrderId = order.Id;
                            originPackingCommand.CodeAx = order.CodeAx;
                            originPackingCommand.OrderIdentifier = order.OrderNumberSequence.ToString();
                            originPackingCommand.TotalZoneCount = itemsOrigin.GroupBy(c => c.PickingZoneId).Count();
                            originPackingCommand.TotalPackage = totalPacking;
                            originPackingCommand.TotalPackageThermolabile = itemsOrigin.Where(c => c.Thermolabile).Sum(c => c.Quantity % c.Packing);
                            originPackingCommand.CustomerName = order.CustomerName;
                            originPackingCommand.SectorCode = customer.SectorCode;
                            originPackingCommand.OrderDate = order.OrderDate;
                            originPackingCommand.SectorName = customer.Sector;
                            originPackingCommand.PreparationOrderItems = _mapper.Map<List<PreparationOrderItemDtoV1>>(itemsOrigin.Where(c => c.Quantity != 0));
                            await _commandBus.SendAsync(originPackingCommand, cancellationToken);
                            // End Colis d'origine
                        }
                        if (itemsBulk.Count > 0)
                        {
                            // PR VRAC NON THERMOLABILE ET NON PSYCHOTROPE
                            foreach (var item in itemsBulk.GroupBy(c => c.ZoneGroupId))
                            {
                                if (item.Key != null)
                                {
                                    var zone = await _commandBus.SendAsync(new GetGroupZoneByIdQuery { Id = item.Key.Value });
                                    var items = item.Select(x => x);
                                    var vracCommand = new CreatePreparationOrderCommand();
                                    vracCommand.ZoneGroupId = item.Key.Value;
                                    vracCommand.OrderIdentifier = order.OrderNumber;
                                    vracCommand.CodeAx = order.CodeAx;
                                    vracCommand.ZoneGroupName = itemsBulk.Where(c => item.Key != null && c.ZoneGroupId == item.Key.Value).Select(c => c.ZoneGroupName).FirstOrDefault();
                                    vracCommand.zoneGroupOrder = zone.Order;
                                    vracCommand.CustomerId = order.CustomerId;
                                    vracCommand.OrderNumberSequence = order.OrderNumberSequence;
                                    vracCommand.OrderIdentifier = order.OrderNumberSequence.ToString();
                                    vracCommand.OrderId = order.Id;
                                    vracCommand.OrderDate = order.OrderDate;
                                    vracCommand.CustomerName = order.CustomerName;
                                    vracCommand.SectorName = customer.Sector;
                                    vracCommand.SectorCode = customer.SectorCode;
                                    vracCommand.TotalZoneCount = items.GroupBy(c => c.PickingZoneId).Count();
                                    vracCommand.TotalPackage = 0;
                                    vracCommand.PreparationOrderItems = _mapper.Map<List<PreparationOrderItemDtoV1>>(items.Where(c => c.Quantity != 0));
                                    if (vracCommand.PreparationOrderItems.Count > 0)
                                        await _commandBus.SendAsync(vracCommand, cancellationToken);
                                }
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        _logger.Error(e.Message);
                        _logger.Error(e.InnerException?.Message);
                    }
                }
                order = await _ordersRepository.Table.Where(x => x.Id == request.OrderId).FirstOrDefaultAsync(cancellationToken);
                order.OrderStatus = OrderStatus.Prepared;
                _ordersRepository.Update(order);
                try
                {
                    await _ordersRepository.UnitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    _logger.Error(ex.InnerException?.Message);
                    return new ValidationResult
                    {
                        Errors =  {new ValidationFailure("Erreur ! ","Erreur de génération des Ops !")}
                    };
                }
                return default;
            }
            return new ValidationResult
            {
                Errors = { new ValidationFailure("Erreur ! ", "Ops déjà générés !") }
            };

        }

        public async Task<CachedOrder> Handle(OrderCreateCommand request, CancellationToken cancellationToken)
        {
            var _list = new List<OrderItemCreateCommand>();
            var activeCustomer = await _commandBus.SendAsync(new GetCustomerByCodeQueryV2
            {
                SupplierOrganizationId = request.OnlineOrderRequest.SupplierId,
                Code = request.OnlineOrderRequest.Code
            }, cancellationToken);
            if (activeCustomer == null)
            {
#region send , you're not a valid customer yet for this supplier
#endregion
                throw new Exception("Client non valide");
                   
            }
            if (activeCustomer.CustomerState == CustomerState.Blocked)
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
                var product = await _commandBus.SendAsync(new GetProductByCode
                {
                    CodeProduct = _.ProductCode
                }, cancellationToken).ConfigureAwait(true);
                if (product != null)
                    _list.Add(
                        new OrderItemCreateCommand
                        {
                            RefDocumentHpcs = request.OnlineOrderRequest.RefDocumentHpcs,
                            DateDocumentHpcs = request.OnlineOrderRequest.DateDocumentHpcs,
                            MinExpiryDate = DateTime.MinValue,
                            ProductCode = _.ProductCode,
                            ProductId = product.Id,
                            CustomerId = customerId,
                            InternalBatchNumber = _.InternalBatchNumber,
                            Quantity = _.Quantity,
                            OrderType = (OrderType)(request.OnlineOrderRequest.Psychotropic ? 1 : 0),
                            SupplierOrganizationId = request.OnlineOrderRequest.SupplierId,
                            SalesPersonId = activeCustomer.SalesPersonId,
                            OrderId=request.OnlineOrderRequest.Id                         
                        }
                        );
            } 
            bool success = false;  
            foreach(var _ in _list)
            {
                var res = await _commandBus.SendAsync(_, cancellationToken).ConfigureAwait(true); 
                if (res == null) success = true;
            };
            if (!success) return null;

            #region get created order
            GetSalesPersonPendingOrderQuery query = new GetSalesPersonPendingOrderQuery
            {
                CustomerId= activeCustomer.CustomerId.Value,
                SalesPersonId= activeCustomer.SalesPersonId??Guid.Empty,
                OrderId=request.OrderId
            };
            var result = await _commandBus.SendAsync(query, cancellationToken);
            var pendingOrders = await _redisCache.GetAsync<List<PendingOrdersModel>>("pending_orders", cancellationToken);
            if (pendingOrders == null)
            {
                await _redisCache.AddOrUpdateAsync<List<PendingOrdersModel>>("pending_orders",
          new List<PendingOrdersModel>
          {
                        new PendingOrdersModel
                        {
                            Id = request.OrderId,
                            CustomerId = activeCustomer.CustomerId.Value,
                            SalesPersonId = activeCustomer.SalesPersonId??Guid.Empty
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
                        CustomerId = activeCustomer.CustomerId.Value,
                        SalesPersonId = activeCustomer.SalesPersonId ?? Guid.Empty
                    });
                    await _redisCache.AddOrUpdateAsync<List<PendingOrdersModel>>("pending_orders", pendingOrders,
                        cancellationToken);
                }
            }
            #region Calculate discounts &  totals
            var draftOrder= _mapper.Map<CachedOrder>(result);
            var key = activeCustomer.CustomerId.Value.ToString() + activeCustomer.SalesPersonId + draftOrder.Id;
            draftOrder.OrderItems.ForEach(async item =>
            {
                var discount= (await _commandBus.SendAsync(new GetActiveDiscountByProductQuery
                    { 
                    OrganizationId=request.SupplierOrganizationId,
                    ProductId=item.ProductId
                }, cancellationToken)).LastOrDefault(disc=>disc.ThresholdQuantity>=item.Quantity);
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
#endregion
        }

        public async Task<ValidationResult> Handle(UpdateOrderDiscountCommandV2 request, CancellationToken cancellationToken)
        {
            string key = request.CustomerId.ToString() + _currentUser.UserId + request.Id;
            await LockProvider<string>.ProvideLockObject(key).WaitAsync(cancellationToken);
            Console.WriteLine($"{key} entred ..");
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == Guid.Empty)
                    throw new InvalidComObjectException("Organization Id should not be null or empty");
                var draftOrder = await GetCurrentPendingOrder(key, request.Id, cancellationToken,
                    orgId);
                if (draftOrder == null)
                    throw new InvalidOperationException("Commande non trouvée");
                foreach (var discountLine in request.DiscountLines)
                {
                    var item = draftOrder.OrderItems.FirstOrDefault(x => x.ProductId == discountLine.ProductId && x.InternalBatchNumber == discountLine.InternalBatchNumber);
                    if (item != null)
                    {
                        item.Discount =Math.Round(discountLine.Discount, 4); 
                    }
                }
                await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
                LockProvider<string>.ProvideLockObject(key).Release();
                Console.WriteLine($"{key} exited ..");

            }
            catch (Exception e)
            {
                _logger.Error( e.Message);
                _logger.Error( e.InnerException?.Message);
                LockProvider<string>.ProvideLockObject(key).Release();
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Removing order error", e.Message)
                    }
                };
            }

            return default;
        }
   
    }
}
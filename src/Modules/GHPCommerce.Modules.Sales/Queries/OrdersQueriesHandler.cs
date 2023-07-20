using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.EntityFrameworkCore;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.Modules.Sales.Commands.DynamicsAx;
using GHPCommerce.Modules.Sales.Models;
using Serilog.Core;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.Core.Shared.Contracts.Hpcs;
using GHPCommerce.Domain.Repositories;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class OrdersQueriesHandler : 
        ICommandHandler<GetOrdersListQuery, PagingResult<OrderDto>>,
        ICommandHandler<GetPendingOrderSupplierQuery, OrderDto>,
        ICommandHandler<GetSalesPersonPendingOrderQuery, OrderDto>,
        ICommandHandler<GetSalesPersonPendingOrderQueryV2, OrderDto>,
        ICommandHandler<GetListOfOrdersOfB2BCustomersQuery, PagingResult<OrderDto>>,
        ICommandHandler<GetOrderByIdQuery, OrderDtoV2>,
        ICommandHandler<GetOrderByIdV1Query, OrderDtoV2>,
        ICommandHandler<GetPendingOrdersForSalePersonQuery, IEnumerable<OrderDto>>,
        ICommandHandler<GetOrderHistoryByProductCodeQuery, IEnumerable<OrderHistoryDto>>,
        ICommandHandler<GetValidOrdersForCustomerQuery, IEnumerable<OrderDto>>,
        ICommandHandler<GetPagedOrdersQuery , SyncPagedResult<OrderDto>>,
        ICommandHandler<GetAllPendingOrdersForSalePersonQuery, IEnumerable<OrderDto>>,
        ICommandHandler<HasOrderToday, bool>,
        ICommandHandler<GetTodayOrderForCustomers, OrderDtoV6>,
        ICommandHandler<GetOrdersByOnlineCustomer,List<OrderTableModel>>,
        ICommandHandler<GetOrderDetailsByOnlineCustomer, List<OrderLineModel>>,
        ICommandHandler<OrderLoopkupByOnlineCustomer, List<string>>,
        ICommandHandler<GetOrderByIdQueryV2, OrderDtoV5>

    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;
        private readonly Logger _logger;
        private readonly IRepository<User, Guid> _userRepository;

        public OrdersQueriesHandler(
            IOrdersRepository ordersRepository,
            IMapper mapper,
            ICommandBus commandBus,
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser,
            ICache redisCache,
            Logger logger, IRepository<User, Guid> userRepository)
        {
            _ordersRepository = ordersRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _redisCache = redisCache;
            _commandBus = commandBus;
            _logger = logger;
            _userRepository = userRepository;
        }

        public async Task<OrderDto> Handle(GetPendingOrderSupplierQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return null;
            string key = request.SupplierId.ToString() + _currentUser.UserId;
            var draftOrder = await _redisCache.GetAsync<CachedOrder>(key, cancellationToken);

            if (draftOrder == null) return null;

            var order = new OrderDto
            {
                Id = draftOrder.Id, 
                SupplierId = draftOrder.SupplierId, 
                OrderNumber = draftOrder.OrderNumber,
                OrderItems =_mapper.Map<List<OrderItem>>(draftOrder.OrderItems)
            };
            
            var orderItems = order.OrderItems.GroupBy(o => o.ProductId)
                .Select(x => new OrderItem{ 
                    ProductId = x.Select( p=>p.ProductId).First(), 
                    Quantity = x.Sum(p=> p.Quantity),
                    UnitPrice = x.Select(p => p.UnitPrice).First(),
                    Tax = x.Select(p => p.Tax).First(),
                    ProductName = x.Select(p =>p.ProductName).First() ,
                    TotalExlTax =  Math.Round(x.Sum(px => px.UnitPrice * px.Quantity * (decimal)(1 - px.Discount)),2),
                    TotalInlTax = Math.Round(x.Sum(px => px.TotalExlTax *(1+ (decimal)px.Tax )),2)
                    
                })
                .ToList();

            order.OrderItems = orderItems;
            
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<PagingResult<OrderDto>> Handle(GetOrdersListQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var total = await _ordersRepository.Table.Where(x => x.CustomerId == orgId).CountAsync(cancellationToken);
            var query = await _ordersRepository.Table.Where(x => x.CustomerId == orgId).Paged(request.Page, request.PageSize).ToListAsync(cancellationToken);
            var data = _mapper.Map<List<OrderDto>>(query);
            return new PagingResult<OrderDto> { Data = data, Total = total };
        }

        public async Task<PagingResult<OrderDto>> Handle(GetListOfOrdersOfB2BCustomersQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return default;
            var currentUser =await _commandBus.SendAsync(new GetUserQuery {Id = _currentUser.UserId, IncludeRoles = true}, cancellationToken);
            
            if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesPerson"))
                return await GetOrdersForSalesPerson(request, orgId, cancellationToken);

            if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesManager") 
                ||currentUser.UserRoles.Any(x => x.Role.Name == "Supervisor"))
                return await GetOrdersForSalesManager(orgId, request, cancellationToken);
            return await GetOrdersForWholesaler(request, cancellationToken, orgId);
        }

        private async Task<PagingResult<OrderDto>> GetOrdersForWholesaler(GetListOfOrdersOfB2BCustomersQuery request,CancellationToken cancellationToken, Guid? orgId)
        {
            var total = await _ordersRepository.Table
                .Where(x => x.SupplierId == orgId)
                .CountAsync(cancellationToken);
            var salesPersonQuery = await _ordersRepository.Table
                .OrderByDescending(x => x.CreatedDateTime)
                .ThenBy(x => x.CustomerId)
                .Where(x => (string.IsNullOrEmpty(request.Term)
                            || x.CustomerName.ToLower().Contains(request.Term.ToLower()))
                            && x.SupplierId == orgId)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken);
            var dataForSalesPerson = _mapper.Map<List<OrderDto>>(salesPersonQuery);
            return new PagingResult<OrderDto> {Data = dataForSalesPerson, Total = total};
        }

        private async Task<PagingResult<OrderDto>> GetOrdersForSalesManager(Guid? orgId, GetListOfOrdersOfB2BCustomersQuery request, CancellationToken cancellationToken)
        {
            if (orgId == null)
                return default;
            var salesPersonIds = await _commandBus.SendAsync(new GetSalesPersonIdsBySalesManageQuery
                {OrganizationId = orgId.Value, UserId = _currentUser.UserId}, cancellationToken);
            var total = await _ordersRepository.Table
                .Where(x => x.SupplierId == orgId && salesPersonIds.Any(s => s == x.DefaultSalesPersonId))
                .CountAsync(cancellationToken);
            var queryForSalesManager = await _ordersRepository.Table
                .OrderByDescending(x => x.CreatedDateTime)
                .ThenBy(x => x.CustomerId)
                .Where(x => string.IsNullOrEmpty(request.Term)
                            || x.CustomerName.ToLower().Contains(request.Term.ToLower())
                            && salesPersonIds.Any(s => s == x.DefaultSalesPersonId))
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken);
            return new PagingResult<OrderDto> {Data = _mapper.Map<List<OrderDto>>(queryForSalesManager), Total = total};
        }

        private async Task<PagingResult<OrderDto>> GetOrdersForSalesPerson(GetListOfOrdersOfB2BCustomersQuery request, Guid? orgId, CancellationToken cancellationToken)
        {
            var total = await _ordersRepository.Table
                .Where(x => x.SupplierId == orgId && x.DefaultSalesPersonId.Value == _currentUser.UserId)
                .CountAsync(cancellationToken);
            var salesPersonQuery = await _ordersRepository.Table
                .OrderByDescending(x => x.CreatedDateTime)
                .ThenBy(x => x.CustomerId)
                .Where(x => (string.IsNullOrEmpty(request.Term)
                            || x.CustomerName.ToLower().Contains(request.Term.ToLower()))
                            && x.SupplierId == orgId 
                            && x.DefaultSalesPersonId.Value == _currentUser.UserId)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken);
            var dataForSalesPerson = _mapper.Map<List<OrderDto>>(salesPersonQuery);
            return new PagingResult<OrderDto> {Data = dataForSalesPerson, Total = total};
        }

        public async Task<OrderDtoV2> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order =  await _ordersRepository
                .Table
                .AsNoTracking()
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (order == null)
                throw new NotFoundException($"Order with id: {request.Id} was not found");
            var result = _mapper.Map<OrderDtoV2>(order);
            var orderItems = result.OrderItems.GroupBy(o => o.ProductId)
               .Select(x => new OrderItemDto
               {
                   ProductId = x.Select(p => p.ProductId).First(),
                   OrderId = x.Select(p => p.OrderId).First(),
                   Discount = x.Select(p => p.Discount).First(),
                   ExpiryDate = x.Select(p => p.ExpiryDate).First(),
                   InternalBatchNumber = x.Select(p => p.InternalBatchNumber).First(),
                   VendorBatchNumber = x.Select(p => p.VendorBatchNumber).First(),
                   ProductCode = x.Select( p => p.ProductCode).First(),
                   Id = x.Select(p => p.Id).First(),
                   UnitPriceInclTax = x.Select(p => p.UnitPriceInclTax).First(),
                   Quantity = x.Sum(p => p.Quantity),
                   UnitPrice = Math.Round(x.Sum(p => p.UnitPrice*p.Quantity)/ x.Sum(p =>p.Quantity)),
                   Tax = x.Select(p => p.Tax).First(),
                   ProductName = x.Select(p => p.ProductName).First(),
                   PFS = x.Select(p => p.PFS).First(),
                   ExtraDiscount = x.Select(p => p.ExtraDiscount).First(),
                   ZoneGroupId = x.Select(p => p.ZoneGroupId).First()

                   // TotalExlTax = Math.Round(x.Sum(px => px.UnitPrice * px.Quantity * (decimal)(1 - px.Discount )), 2),
                   //TotalInlTax = Math.Round(x.Sum(px => px.UnitPrice * px.Quantity * ((decimal)(1 - px.Discount)) * (1 + (decimal)px.Tax)), 2)

               })
               .ToList();

            result.OrderItems = orderItems;
            result.TotalBrut = order.OrderItems.Sum(c => c.UnitPrice * c.Quantity);
            result.TotalDiscountHT = result.TotalBrut - order.OrderDiscount;
            return result;
        }

        public async Task<OrderDto> Handle(GetSalesPersonPendingOrderQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return null;
            string key = request.CustomerId + request.SalesPersonId.ToString() + request.OrderId ;
            var draftOrder = await _redisCache.GetAsync<CachedOrder>(key, cancellationToken);
            if (draftOrder == null) return null;
            return GetOrderDto(draftOrder);

        }
        public async Task<OrderDto> Handle(GetSalesPersonPendingOrderQueryV2 request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return null;
            string key = request.CustomerId.ToString() + _currentUser.UserId;
            var draftOrder = await _redisCache.GetAsync<CachedOrder>(key, cancellationToken);

            if (draftOrder == null) return null;

            return GetOrderDto(draftOrder);
        }

        private OrderDto GetOrderDto(CachedOrder draftOrder)
        {
            var order = new OrderDto
            {
                Id = draftOrder.Id, 
                OrderId = draftOrder.Id,
                SupplierId = draftOrder.SupplierId,
                CustomerId = draftOrder.CustomerId,
                OrderNumber = draftOrder.OrderNumber,
                OrderDate = draftOrder.OrderDate,
                CreatedBy = draftOrder.CreatedBy,
                UpdatedBy = draftOrder.UpdatedBy,
                Psychotropic = draftOrder.Psychotropic,
                OrderItems = _mapper.Map<List<OrderItem>>(draftOrder.OrderItems)
                ,RefDocument = draftOrder.RefDocument,
                RefDocumentHpcs=draftOrder.RefDocumentHpcs,
                DateDocumentHpcs=draftOrder.DateDocumentHpcs,
                Code=draftOrder.Code,
                IsSpecialOrder = draftOrder.IsSpecialOrder
            };

            var orderItems = order.OrderItems
                .Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    OrderId = x.Id,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    Tax = x.Tax,
                    ExpiryDate = x.ExpiryDate,
                    ProductName = x.ProductName,
                    ExtraDiscount = x.ExtraDiscount,
                    Discount = x.Discount,
                    PurchaseUnitPrice = x.PurchaseUnitPrice,
                    ProductCode = x.ProductCode,
                    PickingZoneId =  x.PickingZoneId,
                    PickingZoneName = x.PickingZoneName,
                    ZoneGroupId = x.ZoneGroupId,
                    ZoneGroupName = x.ZoneGroupName,
                    VendorBatchNumber=x.VendorBatchNumber,                   
                    Packing = x.Packing,
                    InternalBatchNumber = x.InternalBatchNumber,
                    TotalExlTax = Math.Round(x.UnitPrice * x.Quantity * (decimal) (1 - (x.Discount +  x.ExtraDiscount)), 2),
                    TotalInlTax = Math.Round(x.UnitPrice * x.Quantity * (decimal)(1 - (x.Discount + x.ExtraDiscount)) * (1 + (decimal) x.Tax), 2)
                })
                .ToList();

            order.OrderItems = orderItems;

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<List<Guid>> GetLookupOrders(string key, CancellationToken cancellationToken)
        {
            return await _redisCache.GetAsync<List<Guid>>(key, cancellationToken);
        }



        public async Task<IEnumerable<OrderDto>> Handle(GetPendingOrdersForSalePersonQuery request, CancellationToken cancellationToken)
        {
            var orders = new List<OrderDto>();
            if (_currentUser.UserId == Guid.Empty)
                return null;
            var firstKey = request.CustomerId.ToString() + _currentUser.UserId;

            var lookupOrders = await GetLookupOrders(firstKey, cancellationToken);
            if (lookupOrders == null) return orders;
            foreach (var lookupOrder in lookupOrders)
            {
                var secondKey = firstKey + lookupOrder;
                var draftOrder = await _redisCache.GetAsync<CachedOrder>(secondKey, cancellationToken);
                if (draftOrder != null)
                {
                    draftOrder.CustomerId = request.CustomerId;
                    orders.Add(GetOrderDto(draftOrder));
                }
            }

            return orders.OrderByDescending(x=>x.OrderDate);
        }

        public async Task<IEnumerable<OrderHistoryDto>> Handle(GetOrderHistoryByProductCodeQuery request, CancellationToken cancellationToken)
        {
                var query = _ordersRepository
                .Table
                .Include(x => x.OrderItems)
                .Where(x => x.CustomerId == request.CustomerId && (x.OrderStatus == OrderStatus.Invoiced  
                                                                   ||x.OrderStatus ==OrderStatus. InShippingArea  
                                                                   ||x.OrderStatus ==OrderStatus. Shipping 
                                                                   ||x.OrderStatus ==OrderStatus.Completed
                                                                   ||x.OrderStatus ==OrderStatus. Consolidated 
                                                                   ||x.OrderStatus ==OrderStatus.Loading 
                                                                   ||x.OrderStatus ==OrderStatus.Prepared 
                                                                   || x.OrderStatus == OrderStatus.Shipped
                                                                   ||x.OrderStatus == OrderStatus.BeingWithdrawn
                                                                   ||x.OrderStatus == OrderStatus.Withdrawn
                                                                   ||x.OrderStatus == OrderStatus.AcknowledgmentOfrReceipt))
                .Select(x => new
                    {x.CustomerName, x.OrderStatus, x.OrderNumberSequence, x.ExpectedShippingDate, x.OrderItems, x.CodeAx});

            var result = new List<OrderHistoryDto>();
            var q = await (from p in query
                let qnt = p.OrderItems.Where(x => x.ProductCode == request.ProductCode).Sum(x => x.Quantity)
                from o in p.OrderItems
                where o.ProductCode == request.ProductCode
                select new OrderHistoryDto
                {
                    ClientName = p.CustomerName,
                    ProductCode = o.ProductCode,
                    OrderNumber = String.IsNullOrEmpty(p.CodeAx)?   p.OrderNumberSequence.ToString() : p.CodeAx,
                    ShippingDate = p.ExpectedShippingDate.Value.Date,
                    Quantity = qnt,
                    ProductName = o.ProductName,
                    CommandStatus =GetOrderStatus((uint) p.OrderStatus)
                }).ToArrayAsync(cancellationToken);
            foreach (var orders in q.GroupBy(x=>x.OrderNumber))
            {
                foreach (var orderHistoryDto in orders)
                {
                    if(result.Any(x=>x.OrderNumber== orderHistoryDto.OrderNumber))
                        continue;
                    result.Add(orderHistoryDto);
                }
            }
            return result;
        }

        public async Task<IEnumerable<OrderDto>> Handle(GetValidOrdersForCustomerQuery request,CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return default;

            var query = await _ordersRepository
                .Table
                .Include(x => x.OrderItems)
                .Where(x => x.CustomerId == request.CustomerId
                            && (x.OrderStatus == OrderStatus.Invoiced  
                                   ||x.OrderStatus ==OrderStatus. InShippingArea  
                                   ||x.OrderStatus ==OrderStatus. Shipping 
                                   ||x.OrderStatus ==OrderStatus.Completed
                                   || x.OrderStatus ==OrderStatus. Consolidated 
                                   ||x.OrderStatus ==OrderStatus.Loading 
                                   ||x.OrderStatus ==OrderStatus.Prepared 
                                   || x.OrderStatus == OrderStatus.BeingWithdrawn
                                   || x.OrderStatus == OrderStatus.Withdrawn
                                   || x.OrderStatus == OrderStatus.Shipped
                                   || x.OrderStatus == OrderStatus.AcknowledgmentOfrReceipt)
                               && x.SupplierId == orgId.Value
                               )
               
                .Select(x => new OrderDto
                {
                    CommandType = x.OrderType == OrderType.Psychotrope ? "Psychotrope" : "Non psychotrope",
                    Status = GetOrderStatus((uint) x.OrderStatus),
                    OrderNumber =x.CodeAx ==string.Empty?   x.OrderNumberSequence.ToString() : x.CodeAx,
                    OrderDate = x.OrderDate,
                    OrderTotal = x.OrderTotal
                })
               
                .ToListAsync(cancellationToken: cancellationToken);
            return query.OrderByDescending(x => x.CreatedDateTime);

        }

        public async Task<SyncPagedResult<OrderDto>> Handle(GetPagedOrdersQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return default;
            var currentUser = await _commandBus.SendAsync(new GetUserQuery {Id = _currentUser.UserId, IncludeRoles = true},
                    cancellationToken);
            // si le role en cours  == Admin , l'utilisateur doit pouvoir visualiser toutes les commandes de vente
            if (currentUser.UserRoles.Any(x => x.Role.Name == "Admin") )
                return await GetPagedOrders(request, orgId, false, null, request.IsPsy, cancellationToken);
            var ids = new List<Guid> { _currentUser.UserId };
            // si le role en cours == salesManager || supervisor on ramène les utilisateurs de son groupe
            if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesManager")
                || currentUser.UserRoles.Any(x => x.Role.Name == "Supervisor"))
            {
                var salesPersonIds = await _commandBus.SendAsync(new GetSalesPersonIdsBySalesManageQuery
                    { OrganizationId = orgId.Value, UserId = _currentUser.UserId }, cancellationToken);
                var personIds = salesPersonIds as Guid[] ?? salesPersonIds.ToArray();
                if (personIds.Any())
                    ids.AddRange(personIds);
            }
         
            // sinon 
            return await GetPagedOrders(request, orgId, true,ids, request.IsPsy, cancellationToken);
        }

        private async Task<SyncPagedResult<OrderDto>> GetPagedOrders(GetPagedOrdersQuery request , Guid? orgId,bool searchByIds,List<Guid> ids, bool? isPsy, CancellationToken cancellationToken)
        {
            
            var salesPersonQuery = _ordersRepository.Table
                .AsNoTracking()
                .Include(x => x.OrderItems)
                .OrderByDescending(x => x.CreatedDateTime)
                .ThenBy(x => x.CustomerId)
                .Where(x => x.SupplierId == orgId)
                .DynamicWhereQuery(request.SyncDataGridQuery);
            if (searchByIds)
                salesPersonQuery = salesPersonQuery.Where(x =>  ids.Any(id=> id == x.CreatedByUserId));
            if(request.Start!= null)
                salesPersonQuery = salesPersonQuery.Where(x => x.OrderDate.Date >= request.Start);
            if (request.End != null)
                salesPersonQuery = salesPersonQuery.Where(x => x.OrderDate.Date <= request.End);
            // gets psychotrop orders for technical directors role
            if (isPsy.HasValue)
            {
                salesPersonQuery = salesPersonQuery.Where(x => x.OrderType == OrderType.Psychotrope && (x.OrderStatus ==  OrderStatus.Accepted || x.OrderStatus == OrderStatus.Pending || x.OrderStatus == OrderStatus.Prepared));
            }

            var total = await salesPersonQuery.CountAsync(cancellationToken: cancellationToken);
            //.ToListAsync(cancellationToken);
            var result = await salesPersonQuery
         //       .OrderByDescending(x => x.CreatedDateTime)
                .Paged(request.SyncDataGridQuery.Skip/ request.SyncDataGridQuery.Take + 1, request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            foreach (var orderOrderItem in result.SelectMany(x => x.OrderItems))
                orderOrderItem.Order = null;
            var dataForSalesPerson = _mapper.Map<List<OrderDto>>(result);
            foreach (var order in dataForSalesPerson)
            {
                order.TotalBrut = order.OrderItems.Sum (c => c.UnitPrice * c.Quantity);
                order.TotalDiscountHT = order.TotalBrut - order.OrderDiscount;
                order.OrderId = order.Id;
                order.Type = order.OrderType != 0 ? "Psychotrope" : "Non psychotrope";
                order.Status = GetOrderStatus(order.OrderStatus);
            }

            return new SyncPagedResult<OrderDto>{ Result = dataForSalesPerson, Count = total};
        }

        private static string GetOrderStatus(uint status)
        {
            string str = status switch
            {
                10 => "EN ATTENTE",
                20 => "Envoyée",
                30 => "Acceptée/Confirmée",
                40 => "En cours de traitement",
                50 => "En route",
                60 => "Terminée",
                70 => "Annulée",
                80 => "Rejetée",
                90 => "Confirmé / Imprimée",
                100 => "Consolidée",
                110 => "En zone d'expédition",
                120 => "Confirmée",
                130 => "En cours de chargement",
                140 => "Facturée",
                150 => "En cours de prélèvement",
                160 => "Prélevée",
                170 => "Accusé de réception",
                180 => "Erreur de syncronisation",
                190 => "Expédiée",
                200 => "Annulée sur AX",
                210 => "Partiellement créée sur AX",
                _ => string.Empty
            };

            return str;
        }

        public async Task<IEnumerable<OrderDto>> Handle(GetAllPendingOrdersForSalePersonQuery request, CancellationToken cancellationToken)
        {
            var result = new List<OrderDto>();
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return default;
            try
            {

                var listOfPendingOrders = await _redisCache.GetAsync<List<PendingOrdersModel>>("pending_orders", cancellationToken);
                if (listOfPendingOrders == null) return result;
                var listOfOrders = new List<Task<OrderDto>>();
                foreach (var item in listOfPendingOrders.Where(x=>x.SalesPersonId == _currentUser.UserId))
                {
                    var firstKey = item.CustomerId.ToString() +_currentUser.UserId;
                    listOfOrders.Add( GetPendingOrder(item.CustomerId, firstKey, item.Id, cancellationToken));
                }
                result= (await Task.WhenAll(listOfOrders)).ToList();
                return result.Where(x=>x!=null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
            }
            return result;
        }
        private async Task<OrderDto> GetPendingOrder(Guid id, string firstKey, Guid lookupOrder, CancellationToken cancellationToken)
        {
            var secondKey = firstKey + lookupOrder;
            var draftOrder = await _redisCache.GetAsync<CachedOrder>(secondKey, cancellationToken);
            if (draftOrder == null) return default;
            draftOrder.CustomerId = id;
            var customer = await _commandBus.SendAsync(new GetCustomerByIdQuery { Id = id }, cancellationToken);
            var order = GetOrderDto(draftOrder);
            order.OrderTotal = order.OrderItems.Sum(x => x.TotalInlTax);
            order.OrderDiscount = order.OrderItems.Sum(x => x.TotalExlTax);
            order.CustomerName = customer?.Name;
            return order;
        }
        public async Task<bool> Handle(HasOrderToday request, CancellationToken cancellationToken)
        {
            return _ordersRepository.Table.Any(x => x.CreatedDateTime.Date == request.Date.Date && x.CustomerId == request.OrganizationId
            /*&& x.CreatedByUserId == request.SalesPersonId */&&x.OrderTotal>0 && x.OrderStatus!=OrderStatus.Canceled
            && x.OrderStatus != OrderStatus.CanceledAx && x.OrderStatus != OrderStatus.Rejected 
            );


        }

        public async Task<OrderDtoV2> Handle(GetOrderByIdV1Query request, CancellationToken cancellationToken)
        {
            var order =  await _ordersRepository
                .Table
                .AsNoTracking()
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (order == null)
                throw new NotFoundException($"Order with id: {request.Id} was not found");
            var result = _mapper.Map<OrderDtoV2>(order);
            return result;
        }

        public async Task<List<OrderTableModel>> Handle(GetOrdersByOnlineCustomer request, CancellationToken cancellationToken)
        {
            var activeCustomer = (await _commandBus.SendAsync(new GetCustomerByCodeQueryV2()
            {
                SupplierOrganizationId = request.OrganizationId,
                Code = request.CustomerCode
            }));
            if (activeCustomer == null)
            {
                #region send , you're not a valid customer yet for this supplier
                #endregion
                throw new Exception("Client non valide");

            }
            Guid customerId = activeCustomer.OrganizationId;
           

            var result = new List<OrderTableModel>(); 



            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return default;
            try
            {

                //if (request.OnlyPendings)
                //{
                //    var listOfPendingOrders = await _redisCache.GetAsync<List<PendingOrdersModel>>("pending_orders", cancellationToken);
                //    if (listOfPendingOrders == null) return result;
                //    var dtos = new List<OrderDto>();
                //    foreach (var item in listOfPendingOrders.Where(x => x.CustomerId == customerId))
                //    {
                //        var firstKey = item.CustomerId.ToString() + item.SalesPersonId;
                //        var dto = await GetPendingOrder(item.CustomerId, firstKey, item.Id, cancellationToken);
                //        if (dto != null)
                //            dtos.Add(dto);
                //    }
                //    dtos = dtos.Where(d => !string.IsNullOrEmpty(d.RefDocumentHpcs)).ToList();
                //    var cachedOrders = _mapper.Map<List<CachedOrder>>(dtos);
                //    foreach (var cached in cachedOrders)
                //    {
                //        result.Add(
                //            new OrderTableModel()
                //            {
                //                StatusPNET = "En attente",
                //                OrderId = Convert.ToInt32(cached.RefDocumentHpcs),
                //                OrderDate = cached.OrderDate,
                //                TotalAmount = cached.OrderTotal,
                //                TotalNetAmountPNET = cached.OrderTotal,
                //                DiscTotValuePNET = cached.OrderDiscount 

                //            }
                //            ); ;
                //    }
                //    return result;
                //}

                result.AddRange( (await _ordersRepository.Table
                    .Where(x => x.CustomerId == customerId && x.SupplierId==request.OrganizationId)
                    
                    
                    .ToListAsync(cancellationToken: cancellationToken))
                    .Select(x =>
                    new OrderTableModel()
                    {
                        StatusPNET = x.OrderStatus == OrderStatus.Accepted ? "En cours de préparation" : (x.OrderStatus == OrderStatus.Completed ? "En attente de livraison" : (
                        x.OrderStatus == OrderStatus.Canceled ? "Annulé" : (
                        x.OrderStatus == OrderStatus.Shipping ? "En route" : (
                        x.OrderStatus == OrderStatus.Shipped ? "Livré" : "Autre"))
                        )),
                        OrderId = string.IsNullOrEmpty(x.RefDocumentHpcs) ? 0 : Convert.ToInt32(x.RefDocumentHpcs),
                        OrderIdPNET = x.OrderNumber.ToString(),
                        ValidationTimePNET = x.CreatedDateTime.DateTime,
                        TotalNetAmountPNET=x.OrderTotal,
                        OrderDate = string.IsNullOrEmpty(x.RefDocumentHpcs)?x.CreatedDateTime.Date:x.DateDocumentHpcs.Value.ToLocalTime(),
                        TotalAmount = x.OrderTotal,
                        DiscTotValuePNET = x.OrderDiscount
                    })
                    .OrderByDescending(x => x.ValidationTimePNET));
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
            }
            return result;
        }

        public async Task<List<OrderLineModel>> Handle(GetOrderDetailsByOnlineCustomer request, CancellationToken cancellationToken)
        {
            var activeCustomer = (await _commandBus.SendAsync(new GetCustomerByCodeQueryV2()
            {
                SupplierOrganizationId = request.OrganizationId,
                Code = request.CustomerCode
            }));
            if (activeCustomer == null)
            {
                #region send , you're not a valid customer yet for this supplier
                #endregion
                throw new Exception("Client non valide");

            }
            Guid customerId = activeCustomer.CustomerId.HasValue ? activeCustomer.CustomerId.Value : Guid.Empty;


            var result = new List<OrderLineModel>();



            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return default;
            try
            {

                var listOfPendingOrders = await _redisCache.GetAsync<List<PendingOrdersModel>>("pending_orders", cancellationToken);
                if (listOfPendingOrders == null) return result;
                var dto = new  OrderDto();
                foreach (var item in listOfPendingOrders.Where(x => x.CustomerId == customerId))
                {
                    var firstKey = item.CustomerId.ToString() + item.SalesPersonId;
                    var dto_ = await GetPendingOrder(item.CustomerId, firstKey, item.Id, cancellationToken);
                    if (dto_ != null && dto_.RefDocumentHpcs.ToLower() == request.OrderNumber.ToLower() 
                        && dto_.DateDocumentHpcs.HasValue && dto_.DateDocumentHpcs.Value.Date==request.OrderDate.Date)
                    {
                        dto = dto_;
                        break;
                    }
                }
                
                var cachedOrder = _mapper.Map<CachedOrder>(dto);
                if (cachedOrder.OrderItems != null && cachedOrder.OrderItems.Count > 0)
                {
                    foreach (var x in cachedOrder.OrderItems)
                    {
                        result.Add(
                            new OrderLineModel()
                            {
                                BatchNumber = x.InternalBatchNumber,
                                ExpiryDate = x.ExpiryDate ?? DateTime.Now,
                                ProductId = x.ProductCode,
                                ProductName = x.ProductName,
                                Qty = x.Quantity,
                                UnitPrice = x.UnitPrice,
                                LineAmount = x.UnitPrice*x.Quantity,
                                DiscValue = (decimal)(x.Discount) + x.ExtraDiscount                                
                            }
                            ); ;

                    }
                    return result;
                }
                var requestOrderSeq = Convert.ToInt32(request.OrderNumber.Substring(request.OrderNumber.LastIndexOf("-") + 1));

                var persistedOrder =
                    await _ordersRepository.Table
                    .Where(x => x.CustomerId == activeCustomer.OrganizationId && x.SupplierId == request.OrganizationId&&
                    
                    x.RefDocumentHpcs == request.OrderNumber && x.DateDocumentHpcs.HasValue &&
                    x.DateDocumentHpcs.Value.Date == request.OrderDate.Date
                   ).Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(cancellationToken);
                if(persistedOrder==null)
                    persistedOrder =
                    await _ordersRepository.Table
                    .Where(x => x.CustomerId == activeCustomer.OrganizationId && x.SupplierId == request.OrganizationId &&
                    
                     requestOrderSeq == x.OrderNumberSequence &&
                    x.OrderDate.Date == request.OrderDate.Date).Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(cancellationToken);


                result.AddRange((persistedOrder.OrderItems.Where(i=>i.AcceptedOnAx)

                    .Select(x =>
                    new OrderLineModel()
                    {
                        BatchNumber=x.InternalBatchNumber,
                        ExpiryDate=x.ExpiryDate??DateTime.Now,
                        ProductId=x.ProductCode,
                        ProductName=x.ProductName,
                        Qty=x.Quantity,
                        UnitPrice=x.UnitPrice,
                        LineAmount=x.TotalInlTax,
                        DiscValue=(decimal)(x.Discount+x.ExtraDiscount)
                    })));
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
            }
            return result;
        }

        public async Task<List<string>> Handle(OrderLoopkupByOnlineCustomer request, CancellationToken cancellationToken)
        {
            var activeCustomer = (await _commandBus.SendAsync(new GetCustomerByCodeQueryV2()
            {
                SupplierOrganizationId = request.OrganizationId,
                Code = request.CustomerCode
            }));
            if (activeCustomer == null)
            {
                #region send , you're not a valid customer yet for this supplier
                #endregion
                throw new Exception("Client non valide");
            }
            Guid customerId = activeCustomer.CustomerId.HasValue ? activeCustomer.CustomerId.Value : Guid.Empty;
            var result = new List<string>();
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return default;
            try
            {

                var listOfPendingOrders = await _redisCache.GetAsync<List<PendingOrdersModel>>("pending_orders", cancellationToken);
                if (listOfPendingOrders != null)
                {
                    var dto = new List<string>();
                    foreach (var item in listOfPendingOrders.Where(x => x.CustomerId == customerId))
                    {
                        var firstKey = item.CustomerId.ToString() + item.SalesPersonId;
                        var dto_ = await GetPendingOrder(item.CustomerId, firstKey, item.Id, cancellationToken);
                        if (dto_ != null && dto_.OrderItems.Any
                            (i => i.ProductName.ToLower().Contains(request.Filter.ToLower())))
                        {
                            dto.Add(dto_.OrderNumber);
                        }
                    }
                }
                 result.AddRange(
                    await _ordersRepository.Table
                    .Where(x => x.CustomerId == customerId && x.SupplierId == request.OrganizationId
                    ).Include(o => o.OrderItems)
                    .Where(o=>o.OrderItems.Any(o=>o.ProductName.ToLower().Contains(request.Filter.ToLower())))
                    .Select(o=>o.OrderNumber).Distinct().ToListAsync(cancellationToken));

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
            }
            return result;

        }

        public async Task<OrderDtoV5> Handle(GetOrderByIdQueryV2 request, CancellationToken cancellationToken)
        {
            return await _ordersRepository.Table
                .AsNoTracking()
                .Where(x => x.Id == request.Id)
                .Select( x=> new OrderDtoV5
                {
                    CreatedBy = x.CreatedBy,
                    OrderType = (int)x.OrderType,
                    CustomerId = x.CustomerId,
                    SequenceNumber = x.OrderNumberSequence,
                    RefDocument = x.RefDocument,
                    CreatedById = x.CreatedByUserId,
                    CodeAx = x.CodeAx,
                    OrderStatus =(int) x.OrderStatus
                })
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
        public async Task<OrderDtoV6> Handle(GetTodayOrderForCustomers request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId.IsNullOrEmpty())
                return null;
            var currentUser =
              await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                  cancellationToken);
            var ids = new List<Guid> { _currentUser.UserId };

            if (currentUser.UserRoles.Any(x => x.Role.Name == "Supervisor"))
            {
                var salesPersonIds = await _commandBus.SendAsync(new GetSalesPersonIdsBySalesManageQuery
                { OrganizationId = orgId.Value, UserId = _currentUser.UserId }, cancellationToken);
                var personIds = salesPersonIds as Guid[] ?? salesPersonIds.ToArray();
                if (personIds.Any())
                    ids.AddRange(personIds);
                var orders =
                    _ordersRepository.Table
                    .Include(ord=>ord.OrderItems)
                    //.AsNoTracking()
                    .Where(order =>
                    (request.CustomerId == null || order.CustomerId == request.CustomerId) &&
                    order.OrderDate.Date == request.Date.Date && ids.Any(c=>c==order.CreatedByUserId)

                     && order.OrderStatus != OrderStatus.Canceled
            && order.OrderStatus != OrderStatus.CanceledAx && order.OrderStatus != OrderStatus.Rejected); 
                if (orders != null)
                {
                    var benefitDay = await orders.SumAsync(x => x.OrderBenefit, cancellationToken);
                    var totalDaily = await orders.SumAsync(x => x.OrderDiscount, cancellationToken);
                    return new OrderDtoV6
                    {
                        OrderTotal = totalDaily,//OrderDiscount is HT NET here,to be kept in mind
                        DailyMarkUpRate = totalDaily == 0 ? 0 : benefitDay / totalDaily
                    };
                }
                else return new OrderDtoV6();
            }
            else
            {
                var orders = _ordersRepository.Table
                    .Include(ord => ord.OrderItems)
                    .AsNoTracking()
                    .Where(x =>
                    (request.CustomerId == null || x.CustomerId == request.CustomerId) &&
                    x.OrderDate.Date == request.Date.Date && x.CreatedByUserId == _currentUser.UserId
                     && x.OrderStatus != OrderStatus.Canceled
            && x.OrderStatus != OrderStatus.CanceledAx && x.OrderStatus != OrderStatus.Rejected

                    );
                if (orders != null)
                {
                    var benefitDay = await orders.SumAsync(x => x.OrderBenefit, cancellationToken);
                    var totalDaily = await orders.SumAsync(x => x.OrderDiscount, cancellationToken);
                    return new OrderDtoV6
                    {
                        OrderTotal = totalDaily,//OrderDiscount is HT NET here,to be kept in mind
                        DailyMarkUpRate = totalDaily == 0 ? 0 : benefitDay / totalDaily
                    };
                }
                else  return new OrderDtoV6();
            }

        }
    }
}

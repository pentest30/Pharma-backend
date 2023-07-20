using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Models;
using Serilog.Core;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetPendingOrdersForSupervisors : ICommand<IEnumerable<OrderDto>>
    {
    }
    public class  GetPendingOrdersForSupervisorsHandler : ICommandHandler<GetPendingOrdersForSupervisors, IEnumerable<OrderDto>>
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICache _redisCache;
        private readonly Logger _logger;

        public GetPendingOrdersForSupervisorsHandler(ICommandBus commandBus,
            ICurrentOrganization currentOrganization,
            ICache redisCache,
            Logger logger)
        {
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _redisCache = redisCache;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDto>> Handle(GetPendingOrdersForSupervisors request, CancellationToken cancellationToken)
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
                foreach (var item in listOfPendingOrders)
                {
                    var firstKey = item.CustomerId.ToString() +item.SalesPersonId;
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
            order.CustomerName = customer?.Name;
            return order;
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
                RefDocument = draftOrder.RefDocument,
                CreatedByUserId = draftOrder.CreatedByUserId
            };

            order.OrderItems = draftOrder.OrderItems
                .Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    OrderId = x.Id,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    Tax = x.Tax,
                    ExpiryDate = x.ExpiryDate,
                    ProductName = x.ProductName,
                    ExtraDiscount = (double)x.ExtraDiscount,
                    Discount = x.Discount,
                    PurchaseUnitPrice = x.PurchaseUnitPrice,
                    ProductCode = x.ProductCode,
                    PickingZoneId = x.PickingZoneId,
                    PickingZoneName = x.PickingZoneName,
                    ZoneGroupId = x.ZoneGroupId,
                    ZoneGroupName = x.ZoneGroupName,
                    Packing = x.Packing,
                    InternalBatchNumber = x.InternalBatchNumber
                })
                .ToList();

            order.OrderDiscount =Math.Round(draftOrder.OrderItems.Sum(px =>px.UnitPrice * px.Quantity * (decimal)(1 - px.Discount) * (1 - px.ExtraDiscount)), 2);
            order.OrderTotal = Math.Round( draftOrder.OrderItems.Sum(px => px.UnitPrice * px.Quantity * (decimal)(1 - px.Discount) * (1 - px.ExtraDiscount) *(1 + (decimal)px.Tax)), 2);
            return order;
        }

    }
}
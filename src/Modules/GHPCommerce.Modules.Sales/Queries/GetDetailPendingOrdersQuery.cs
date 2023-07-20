using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.PreparationOrder.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Modules.Sales.Models;
using Serilog.Core;


namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetDetailPendingOrdersQuery  : ICommand<List<DetailPendingOrder>>
    {
    }

    public class GetReservedQuantitiesQuery : ICommand<List<DetailPendingOrder>>
    {
        
    }

    public class GetReservedQuantitiesQueryHandler : ICommandHandler<GetReservedQuantitiesQuery, List<DetailPendingOrder>>
    {
        private readonly ICommandBus _commandBus;

        public GetReservedQuantitiesQueryHandler(ICommandBus commandBus,
            ICurrentUser currentUser, Logger logger)
        {
            _commandBus = commandBus;
        }
        public async Task<List<DetailPendingOrder>> Handle(GetReservedQuantitiesQuery request, CancellationToken cancellationToken)
        {
            var pendingOrders = await _commandBus.SendAsync(new GetPendingOrdersForSupervisors(), cancellationToken);
            var poItemsNotControlled = await _commandBus.SendAsync(new GetPOItemsQuery(), cancellationToken);
            var result = new List<DetailPendingOrder>();
            foreach (var preparationOrderItemDtoV1 in poItemsNotControlled)
            {
                result.Add(new DetailPendingOrder
                {
                    InternalBatchNumber = preparationOrderItemDtoV1.InternalBatchNumber,
                    Quantity = preparationOrderItemDtoV1.Quantity,
                    ProductCode = preparationOrderItemDtoV1.ProductCode,
                    ProductName = preparationOrderItemDtoV1.ProductName,
                    OrderNumber = preparationOrderItemDtoV1.OrderNumberSequence.ToString()
                    
                });
            }

            if (pendingOrders != null)
            {
                foreach (var pendingOrder in pendingOrders)
                {
                    foreach (var pendingOrderOrderItem in pendingOrder.OrderItems)
                    {
                        result.Add(new DetailPendingOrder
                        {
                            InternalBatchNumber = pendingOrderOrderItem.InternalBatchNumber,
                            Quantity = pendingOrderOrderItem.Quantity,
                            ProductCode = pendingOrderOrderItem.ProductCode,
                            ProductName = pendingOrderOrderItem.ProductName,
                            OrderNumber = pendingOrder.OrderNumber + "# en attente"
                    
                        });
                    }
                }
            }

            return result.OrderBy(x=>x.ProductName).ToList();
        }
    }

    public class  GetDetailPendingOrdersQueryHandler : ICommandHandler<GetDetailPendingOrdersQuery, List<DetailPendingOrder>>
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly Logger _logger;

        public GetDetailPendingOrdersQueryHandler(ICommandBus commandBus,
            ICurrentUser currentUser, Logger logger)
        {
            _commandBus = commandBus;
            _currentUser = currentUser;
            _logger = logger;
        }
        public async Task<List<DetailPendingOrder>> Handle(GetDetailPendingOrdersQuery request, CancellationToken cancellationToken)
        {
            var result = new List<DetailPendingOrder>();
            try
            {
                var pendingOrders = await _commandBus.SendAsync(new GetPendingOrdersForSupervisors(), cancellationToken);
                foreach (var orderDto in pendingOrders.Where(x=>x.CreatedByUserId != _currentUser.UserId))
                {
                    foreach (var item in orderDto.OrderItems)
                    {
                        if (!result.Any(x =>
                                x.InternalBatchNumber == item.InternalBatchNumber && x.ProductCode == item.ProductCode &&
                                x.SalesPersonName == orderDto.CreatedBy))
                        {
                            result.Add( new DetailPendingOrder {InternalBatchNumber = item.InternalBatchNumber, Quantity = item.Quantity, ProductName = item.ProductName, ProductCode = item.ProductCode, SalesPersonName = orderDto.CreatedBy});
                        }
                        else
                        {
                            var detail = result.FirstOrDefault(x =>
                                x.InternalBatchNumber == item.InternalBatchNumber && x.ProductCode == item.ProductCode &&
                                x.SalesPersonName == orderDto.CreatedBy);
                            if(detail!=null) detail.Quantity += item.Quantity;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                _logger.Error($"Commandes réservées : {e.Message}");
            }
            return result;
        }
    }
}
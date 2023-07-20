using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;

namespace GHPCommerce.Modules.Sales.Queries.OnlineOrder
{
    public class GetOrdersForCustomerQuery :ICommand<SyncPagedResult<OrderDto>>

    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }

    }

    public class  GetOrdersForCustomerQueryHandler : ICommandHandler<GetOrdersForCustomerQuery, SyncPagedResult<OrderDto>>
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;
        private readonly Logger _logger;

        public GetOrdersForCustomerQueryHandler(
            IOrdersRepository ordersRepository,
            IMapper mapper,
            ICommandBus commandBus,
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser, 
            ICache redisCache,
            Logger logger)
        {
            _ordersRepository = ordersRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _redisCache = redisCache;
            _commandBus = commandBus;
            _logger = logger;

        }

        public async Task<SyncPagedResult<OrderDto>> Handle(GetOrdersForCustomerQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return default;
            var query = _ordersRepository.Table
                .AsNoTracking()
                .Include(x => x.OrderItems)
                .OrderByDescending(x => x.CreatedDateTime)
                .Where(x => x.CustomerId == orgId)
                .DynamicWhereQuery(request.SyncDataGridQuery);
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query
                .Paged(request.SyncDataGridQuery.Skip/ request.SyncDataGridQuery.Take + 1, request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<OrderDto>>(result);
            foreach (var orderOrderItem in data.SelectMany(x => x.OrderItems))
                orderOrderItem.Order = null;
            foreach (var order in data)
            {
                
                order.TotalBrut = order.OrderItems.Sum (c => c.UnitPrice * c.Quantity);
                order.TotalDiscountHT = order.TotalBrut - order.OrderDiscount;
                order.OrderId = order.Id;
                order.Type = order.OrderType != 0 ? "Psychotrope" : "Non psychotrope";
            }

            return new SyncPagedResult<OrderDto>{ Result = data, Count = total};
        }
    }

}
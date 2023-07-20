using System;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Shared.Contracts.PreparationOrder.Queries;

namespace GHPCommerce.Modules.Sales.Queries.Shared
{
    public class SharedOrderQueriesHandler :
        ICommandHandler<GetOrdersByStatusQuery, SyncPagedResult<OrderDtoV4>>,
        ICommandHandler<GetSharedOrderById, OrderDtoV3>
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;

        public SharedOrderQueriesHandler(
            IOrdersRepository ordersRepository,
            IMapper mapper,
            ICommandBus commandBus,
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser,
            ICache redisCache)
        {
            _ordersRepository = ordersRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _redisCache = redisCache;
            _commandBus = commandBus;

        }

        public async Task<SyncPagedResult<OrderDtoV4>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            
                if (!org.HasValue)
                    return new SyncPagedResult<OrderDtoV4>();
                var currentUser =
                    await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                        cancellationToken);
                var query = _ordersRepository.Table.Where(x =>  x.OrderStatus == (OrderStatus)request.Status);
                var opNotPrinted = await _commandBus.SendAsync(new GetNotPrintedOrders() {DataGridQuery = request.DataGridQuery});
                query = query.Where(c => opNotPrinted.Contains(c.Id));
                query = query.DynamicWhereQuery(request.DataGridQuery);

                var total = await query.CountAsync(cancellationToken: cancellationToken);
                query = query
                    .OrderByDescending(x => x.CreatedDateTime)
                    .Paged(request.DataGridQuery.Skip / request.DataGridQuery.Take + 1, request.DataGridQuery.Take);
                var result = _mapper.Map<List<OrderDtoV4>>(await query.ToListAsync(cancellationToken));
                foreach (var item in result)
                {
                    var ops = await _commandBus.SendAsync(new GetStateBlByOrderQuery() {OrderId = item.Id});
                    item.SectorName = ops.First().SectorName;
                    item.SectorCode = ops.First().SectorCode;
                    item.OrderId = item.Id;
                    item.ZoneGroupName = string.Join("/",ops.Select(c => c.ZoneGroupName).Distinct().ToList());

                
                }
                return new SyncPagedResult<OrderDtoV4>
                {
                    Count = total,
                    Result = result
                };
            }
            catch (Exception e)
            {
                throw e;
            }
          
        }

        public async Task<OrderDtoV3> Handle(GetSharedOrderById request, CancellationToken cancellationToken)
        {
            var order = await _commandBus.SendAsync(new GetOrderByIdQuery { Id = request.OrderId }, cancellationToken);
            return _mapper.Map<OrderDtoV3>(order);
        }
    }
}

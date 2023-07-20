using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;
using GHPCommerce.Core.Shared.PreparationOrder.Queries;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.Repositories;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Modules.Shared.Contracts.PreparationOrder.Queries;
using PreparationOrderStatus = GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrderStatus;
using GHPCommerce.Modules.PreparationOrder.Entities;

namespace GHPCommerce.Modules.PreparationOrder.Queries
{

    class PreparationOrdersQueriesHandler :
        ICommandHandler<GetPagedPreparationOrdersQuery, SyncPagedResult<PreparationOrdersDto>>,
        ICommandHandler<GetPagedOpForConsolidationQuery, SyncPagedResult<PreparationOrdersDto>>,
        ICommandHandler<GetPagedOpForControllerQuery, SyncPagedResult<PreparationOrderDtoV4>>,
        ICommandHandler<GetArchivedBlByOrderQuery, SyncPagedResult<PreparationOrderDtoV4>>,
        ICommandHandler<GetPreparationOrderByIdQuery, PreparationOrdersDtoV2>,
        ICommandHandler<GetControlledBlByOrderQuery, PreparationOrderDtoV3>,
        ICommandHandler<GetStateBlByOrderQuery, List<PreparationOrderDtoV5>>,
        ICommandHandler<GetNotPrintedOrders, List<Guid>>,
        ICommandHandler<GetPOsByOrderQuery, List<PreparationOrderItemDtoV1>>

    {
        private readonly IPreparationOrderRepository _preparationOrdersRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public PreparationOrdersQueriesHandler(
          IPreparationOrderRepository preparationOrdersRepository,
          ICurrentOrganization currentOrganization,
          ICurrentUser currentUser,
          ICommandBus commandBus,
          IMapper mapper)
        {
            _preparationOrdersRepository = preparationOrdersRepository;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _commandBus = commandBus;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<PreparationOrdersDto>> Handle(GetPagedPreparationOrdersQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<PreparationOrdersDto>();
            var currentUser =
                await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                    cancellationToken);
            var query = _preparationOrdersRepository.Table.Where(x => x.OrganizationId == org);
            
            if(!String.IsNullOrEmpty(request.barCode)) query = query.Where(c => c.BarCode == request.barCode);

            ////if (currentUser.UserRoles.Any(x => x.Role.Name == "PrintingAgent"))
            query = query
                .Where(c => !c.Printed)
                .DynamicWhereQuery(request.DataGridQuery);
           


            var total = await query.CountAsync(cancellationToken: cancellationToken);
            query = query
            .Paged(request.DataGridQuery.Skip / request.DataGridQuery.Take + 1, request.DataGridQuery.Take);
            var preparationOrdersList = _mapper.Map<List<PreparationOrdersDto>>(await query.ToListAsync(cancellationToken));
            return new SyncPagedResult<PreparationOrdersDto>
            {
                Count = total,
                Result = preparationOrdersList
            };
        }
        public async Task<SyncPagedResult<PreparationOrderDtoV4>> Handle(GetPagedOpForControllerQuery request, CancellationToken cancellationToken)
        {
            if (request.BarCode.Length > 13)
                request.BarCode = request.BarCode.Substring(0, 13);
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<PreparationOrderDtoV4>();
            var currentUser =
                await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                    cancellationToken);
            var te = _preparationOrdersRepository.Table 
                .Where(x => 
                    x.OrganizationId == org &&
                    x.Printed && 
                    (x.PreparationOrderStatus == PreparationOrderStatus.Valid || x.PreparationOrderStatus == 0))
                .DynamicWhereQuery(request.DataGridQuery);
            if (!String.IsNullOrEmpty(request.BarCode) && request.BarCode.Length >=  13)
            {
                te = te.Where(c => c.BarCode == request.BarCode.Substring(0,13));
            }
           
            var query = te
                .Include(c => c.PreparationOrderItems)
                .SelectMany(c => c.PreparationOrderItems);
            if (!String.IsNullOrEmpty(request.BarCode) && request.BarCode.Length >=  13)
            {
                te = te.Where(c => c.BarCode == request.BarCode.Substring(0,13));
            }
            if(!String.IsNullOrEmpty(request.BarCode) && (request.BarCode.Length > 14 || request.BarCode.Length <13 ) )
            {
                return new SyncPagedResult<PreparationOrderDtoV4>
                {
                    Count = 0,
                    Result = new List<PreparationOrderDtoV4> { }
                };
            }
            if (request.DataGridQuery.Where != null)
            {
                foreach (var wherePredicate in request.DataGridQuery.Where[0].Predicates)
                {
                    if (wherePredicate.Field == "pickingZoneName")
                    {
                        query = query.Where(c => c.PickingZoneName == wherePredicate.Value.ToString());

                    }
                }
            }
            if (!String.IsNullOrEmpty(request.BarCode) && request.BarCode.Length == 14)
            {
                var zone = int.Parse(request.BarCode.Substring(13, 1));
                query = query.Where(c => c.PickingZoneOrder == zone);
            }
            //query = query.Where(x => !x.IsControlled);
            var total = (await query.ToListAsync(cancellationToken))
                .GroupBy(c => new { c.PickingZoneName, c.OrderId })
                .Count();

            var preparationOrdersList = (await query.ToListAsync(cancellationToken))
                .GroupBy(c => new { c.PickingZoneName, c.OrderId })
                .Distinct()
                .AsQueryable()
                .Paged(request.DataGridQuery.Skip / request.DataGridQuery.Take + 1, request.DataGridQuery.Take);

            var result = new List<PreparationOrderDtoV4> { };

            foreach (var item in preparationOrdersList)
            {
                var op = await _preparationOrdersRepository.Table
                    .Where(x => x.OrganizationId == org && x.Id == item.Select(c => c.PreparationOrderId).First())
                    .Include(x => x.PreparationOrderExecuters)
                    .Include(x => x.PreparationOrderVerifiers)
                    .FirstOrDefaultAsync();
                result.Add(new PreparationOrderDtoV4
                {
                    Id = op.Id,
                    PickingZoneId = item.Select(c => c.PickingZoneId).FirstOrDefault().Value,
                    PickingZoneName = item.Select(c => c.PickingZoneName).FirstOrDefault(),
                    CustomerName = op.CustomerName,
                    OrderIdentifier = op.OrderIdentifier,
                    OrganizationName = op.OrganizationName,
                    OrderDate = op.OrderDate,
                    OrderId = op.OrderId,
                    BarCode = op.BarCode,
                    CountVerifiers  = op.PreparationOrderVerifiers.Where(c => c.PickingZoneId == item.Select(t => t.PickingZoneId).FirstOrDefault().Value).Count(),
                    CountExecuters = op.PreparationOrderExecuters.Where(c => c.PickingZoneId == item.Select(t => t.PickingZoneId).FirstOrDefault().Value).Count(),
                    CountNotControlled = preparationOrdersList.Count( c => 
                        !c.First().IsControlled
                        && item.First().PreparationOrderId == op.Id
                        && item.Select(t => t.PickingZoneId).FirstOrDefault().Value == c.First().PickingZoneId ),
                    PreparationOrderStatus = op.PreparationOrderStatus,
                    TotalPackage=op.TotalPackage,
                    TotalPackageThermolabile=op.TotalPackageThermolabile 
                });
            }

            
            return new SyncPagedResult<PreparationOrderDtoV4>
            {
                Count = total,
                Result = result
            };


        }
        public async Task<PreparationOrdersDtoV2> Handle(GetPreparationOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var preparationOrder = await _preparationOrdersRepository
                .Table
                .AsNoTracking()
                .Include(x => x.PreparationOrderItems)
                .Include(x => x.PreparationOrderVerifiers)
                .Include(x => x.PreparationOrderExecuters)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (preparationOrder == null)
                throw new NotFoundException($"Preparation Order with id: {request.Id} was not found");
            var result = _mapper.Map<PreparationOrdersDtoV2>(preparationOrder);
           
            return result;
        }

        public async Task<PreparationOrderDtoV3> Handle(GetControlledBlByOrderQuery request, CancellationToken cancellationToken)
        {
            var query = _preparationOrdersRepository
                .Table
                .Where(c => c.OrderId == request.OrderId );
            var data = await query
                .Include(x => x.PreparationOrderItems)
                
                .ToListAsync(cancellationToken);
            var CountBlNotControlled = query.Count() - data.Count();

            var items = new List<PreparationOrderItemDto>() { };
            foreach (var bl in data)
            {
                var blMapped = _mapper.Map<PreparationOrdersDtoV2>(bl);
                items.AddRange(blMapped.PreparationOrderItems);
            }
            return new PreparationOrderDtoV3
            {
                CountBlNotControlled = items.Where(c => c.PickingZoneId == request.PickingZoneId && !c.IsControlled).Count(),
                items = items
            };
        }

        public async Task<SyncPagedResult<PreparationOrderDtoV4>> Handle(GetArchivedBlByOrderQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<PreparationOrderDtoV4>();
            var query1 = (from op in _preparationOrdersRepository.Table.AsQueryable()
                        .AsNoTracking()
                        .Include(c => c.PreparationOrderItems)
                        .Include(x => x.PreparationOrderVerifiers)
                    from opItem in op.PreparationOrderItems
                    from vp in op.PreparationOrderVerifiers.DefaultIfEmpty()
                    where  op.OrganizationId == org && opItem.PickingZoneId == vp.PickingZoneId
                    select new  
                    {
                        op.Id,
                        PickingZoneId = opItem.PickingZoneId.Value,
                        opItem.PickingZoneName,
                        op.CustomerName,
                        op.ZoneGroupName,
                        op.OrderIdentifier,
                        op.OrganizationName,
                        op.OrderDate,
                        op.OrderId,
                        op.PreparationOrderStatus,
                        op.BarCode,
                        opItem.PickingZoneOrder,
                        op.TotalPackage,
                        op.TotalPackageThermolabile,
                        vp.VerifiedByName,
                        vp.VerifiedTime
                    }
                );


            if (request.DataGridQuery.Where != null)
            {
                foreach (var wherePredicate in request.DataGridQuery.Where[0].Predicates)
                {
                    
                    if (wherePredicate.Field == "verifiedByName")
                    {
                        query1 = query1.Where(x => x.VerifiedByName.Contains(wherePredicate.Value.ToString()));
                    }
                    if (wherePredicate.Field == "pickingZoneName")
                    {
                        query1 = query1.Where(x =>x.PickingZoneName.Contains(wherePredicate.Value.ToString()));
                    }
                    if (wherePredicate.Field == "customerName")
                    {
                        query1 = query1.Where(x =>x.CustomerName.Contains(wherePredicate.Value.ToString()));
                    }
                    if (wherePredicate.Field == "orderIdentifier")
                    {
                        query1 = query1.Where(x =>x.OrderIdentifier.Contains(wherePredicate.Value.ToString()));
                    }
                }
            }
            

            var total = await query1.CountAsync(cancellationToken: cancellationToken);

            var result = query1
                .Paged(request.DataGridQuery.Skip / request.DataGridQuery.Take + 1, request.DataGridQuery.Take)
                .Select(x => new PreparationOrderDtoV4
                {
                    Id = x.Id,
                    PickingZoneId = x.PickingZoneId,
                    PickingZoneName = x.PickingZoneName,
                    CustomerName = x.CustomerName,
                    ZoneGroupName = x.ZoneGroupName,
                    OrderIdentifier = x.OrderIdentifier,
                    OrganizationName = x.OrganizationName,
                    OrderDate = x.OrderDate,
                    OrderId = x.OrderId,
                    PreparationOrderStatus = x.PreparationOrderStatus,
                    BarCode = x.BarCode,
                    PickingZoneOrder=x.PickingZoneOrder,
                    TotalPackage = x.TotalPackage,
                    TotalPackageThermolabile = x.TotalPackageThermolabile,
                    VerifiedByName = x.VerifiedByName,
                    VerifiedTime = x.VerifiedTime
                })
                .ToListAsync(cancellationToken: cancellationToken);

            return new SyncPagedResult<PreparationOrderDtoV4>
            {
                Count = total,
                Result = await result
            };

        }

        public async Task<SyncPagedResult<PreparationOrdersDto>> Handle(GetPagedOpForConsolidationQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<PreparationOrdersDto>();
            var currentUser =
                await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                    cancellationToken);
            var query = _preparationOrdersRepository.Table
                .Where(x => x.OrganizationId == org && (x.PreparationOrderStatus == PreparationOrderStatus.Controlled || x.PreparationOrderStatus == PreparationOrderStatus.Consolidated))
                .DynamicWhereQuery(request.DataGridQuery);

            if (!String.IsNullOrEmpty(request.barCode)) query = query.Where(c => c.BarCode == request.barCode);
            
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            query = query
                 .Paged(request.DataGridQuery.Skip / request.DataGridQuery.Take + 1, request.DataGridQuery.Take);
            var preparationOrdersList = _mapper.Map<List<PreparationOrdersDto>>(await query.ToListAsync(cancellationToken));
            foreach (var item in preparationOrdersList)
            {
                var ops = await _preparationOrdersRepository.Table.Where(c => c.OrderId == item.OrderId ).ToListAsync();
                var count = ops.Where(c => c.PreparationOrderStatus == PreparationOrderStatus.Consolidated).Count();
                item.IsConsolidated = ops.Count().Equals(count);
            }
            return new SyncPagedResult<PreparationOrdersDto>
            {
                Count = total,
                Result = preparationOrdersList
            };
        }

        public async Task<List<PreparationOrderDtoV5>> Handle(GetStateBlByOrderQuery request, CancellationToken cancellationToken)
        {
            var query = await _preparationOrdersRepository
                .Table
                .Where(c => c.OrderId == request.OrderId)
                .ToListAsync(cancellationToken: cancellationToken);
            var result = new List<PreparationOrderDtoV5> { };
            foreach (var item in query)
            {
                result.Add(new PreparationOrderDtoV5
                {
                    ConsolidatedByName = item.EmployeeCode,
                    ConsolidatedTime = item.ConsolidatedTime,
                    ZoneGroupName = item.ZoneGroupName,
                    SectorName =  item.SectorName,
                    SectorCode = item.SectorCode,
                    OrderId = item.OrderId,
                    TotalPackage =item.TotalPackage,
                    TotalPackageThermolabile = item.TotalPackageThermolabile ,
                    PreparationOrderStatus = _mapper.Map<Core.Shared.Contracts.PreparationOrders.DTOs.PreparationOrderStatus>(item.PreparationOrderStatus)
                });
            }
            return result;
        }

        public async Task<List<Guid>> Handle(GetNotPrintedOrders request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new List<Guid>();
            var currentUser =
                await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                    cancellationToken);
            var query =  _preparationOrdersRepository.Table.Where(x => x.OrganizationId == org && !x.Printed).AsQueryable();
            query = query.DynamicWhereQuery(request.DataGridQuery);
            return await query.Select(x => x.OrderId).ToListAsync(cancellationToken: cancellationToken);

        }

        public async Task<List<PreparationOrderItemDtoV1>> Handle(GetPOsByOrderQuery request, CancellationToken cancellationToken)
        {
            var items = _preparationOrdersRepository.Table
                .Include(x => x.PreparationOrderItems)
                .Where(x => x.OrderId == request.OrderId);
            var result = await (from item in items
                from preparationOrder in item.PreparationOrderItems
                select new PreparationOrderItemDtoV1
                {
                    ProductId = preparationOrder.ProductId,
                    Quantity = preparationOrder.Quantity,
                    InternalBatchNumber = preparationOrder.InternalBatchNumber,
                    IsControlled = preparationOrder.IsControlled, 
                    OldQuantity = preparationOrder.OldQuantity,
                    Status=(Core.Shared.Events.PreparationOrder.BlStatus)preparationOrder.Status,
                })
                .ToListAsync(cancellationToken: cancellationToken);
            return result;
        }
    }
}

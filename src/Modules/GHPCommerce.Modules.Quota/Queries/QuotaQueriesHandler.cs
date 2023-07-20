using System;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Quota.DTOs;
using GHPCommerce.Modules.Quota.Repositories;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GHPCommerce.Application.Catalog.Products.Queries;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Quota.Entities;
using Microsoft.EntityFrameworkCore;
using LinqKit;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace GHPCommerce.Modules.Quota.Queries
{
    public class QuotaQueriesHandler : 
        ICommandHandler<GetPagedQuotasQuery, SyncPagedResult<QuotaDto>>,
        ICommandHandler<GetQuotaByProductIdQuery, QuotaDtoV1>,
        ICommandHandler<GetQuotaByCustomerQuery, IEnumerable<QuotaDtoV1>>,
        ICommandHandler<GetPagedQuotasByProductQuery,SyncPagedResult<QuotaDto>>,
        ICommandHandler<GetCurrentUser, Guid>,
        ICommandHandler<GetQuotasByProductQuery, IEnumerable<QuotaDto>>,
        ICommandHandler<GetDetailedQuotaQuery, IEnumerable<QuotaDto>>, 
        ICommandHandler<GetQuotasByProductQueryV2, int>,
        ICommandHandler<GetQuotasForSalesPersonByProductIdQuery, Int32>, ICommandHandler<GetQuotasByProductQueryV3, Int32>
    {
        private readonly IQuotaRepository _quotaRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;
        private readonly IRepository<QuotaInitState, Guid> _quotaInitRepository;

        public QuotaQueriesHandler(
            IQuotaRepository quotaRepository,
            ICurrentOrganization currentOrganization, 
            ICurrentUser currentUser ,
            ICommandBus commandBus, 
            IMapper mapper, IRepository<QuotaInitState, Guid> quotaInitRepository)
        {
            _quotaRepository = quotaRepository;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _commandBus = commandBus;
            _mapper = mapper;
            _quotaInitRepository = quotaInitRepository;
        }

        public async Task<SyncPagedResult<QuotaDto>> Handle(GetPagedQuotasQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<QuotaDto>();
            var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },  cancellationToken);
            var query = _quotaRepository.Table 
                .Include(x => x.QuotaTransactions)
                .AsNoTracking()
                .Where(x => x.OrganizationId == org)
                .DynamicWhereQuery(request.DataGridQuery);
            if (currentUser.UserRoles.Any(x => x.Role.Name == "Admin") ||
                currentUser.UserRoles.Any(x => x.Role.Name == "Buyer"))
            {

            }
            else if (currentUser.UserRoles.Any(x => x.Role.Name == "Supervisor"))
            {
                var ids = await _commandBus.SendAsync(new GetSalesPersonsBySupervisorQuery { Id = currentUser.Id },
                    cancellationToken);
                var enumerable = ids as Guid[] ?? ids.ToArray();
                var predicateBuilder = PredicateBuilder.New<Entities.Quota>();
                foreach (var id in enumerable)
                {
                    predicateBuilder.Or(x => x.SalesPersonId == id);
                }
                predicateBuilder.Or(x => x.SalesPersonId == _currentUser.UserId);
                query = query.Where(predicateBuilder);
            }
            else if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesPerson"))
                query = query.Where(x => x.SalesPersonId == currentUser.Id);

            var total = await query.CountAsync(cancellationToken: cancellationToken);

            var result = await query 
                .Paged(request.DataGridQuery.Skip/ request.DataGridQuery.Take + 1, request.DataGridQuery.Take)    
                .ToListAsync(cancellationToken: cancellationToken);

            foreach (var quotaQuotaTransaction in result.SelectMany(quota => quota.QuotaTransactions))
            {
                quotaQuotaTransaction.Quota = null;
            }
            var data = _mapper.Map<List<QuotaDto>>(result);
            return new SyncPagedResult<QuotaDto>{ Count =total, Result = data };
        }

        public async Task<QuotaDtoV1> Handle(GetQuotaByProductIdQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return default;
            var entity = await _quotaRepository.Table
                .FirstOrDefaultAsync(x => x.ProductId == request.ProductId
                                          && x.QuotaDate.Date == request.Date.Date 
                                          &&x.OrganizationId == org
                                          && x.AvailableQuantity>0, cancellationToken);

            return  entity!= null? new QuotaDtoV1 {AvailableQuantity = entity.AvailableQuantity, Id = entity.Id} : default;
        }

        public async Task<IEnumerable<QuotaDtoV1>> Handle(GetQuotaByCustomerQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return default;
            var entities = await _quotaRepository
                .Table
                .AsNoTracking()
                .Where(x => x.OrganizationId == org
                            && x.SalesPersonId == _currentUser.UserId)
                .Select(x => new {x.ProductId, x.AvailableQuantity})
                .ToListAsync(cancellationToken);
            var quotaProducts = await _commandBus.SendAsync(new GetQuotaProductsQuery(), cancellationToken);

            return entities
                .Where(x=> quotaProducts.Any(p=> p.Id == x.ProductId))
                .GroupBy(x => x.ProductId) 
                .Select(items => new QuotaDtoV1
                    {AvailableQuantity = items.Sum(x => x.AvailableQuantity), Id = items.Key})
                .ToList();
        }

        public async Task<SyncPagedResult<QuotaDto>> Handle(GetPagedQuotasByProductQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return default;
            //var currentUser = await _commandBus.SendAsync(new GetUserQuery {Id = _currentUser.UserId, IncludeRoles = true},cancellationToken);
          
            var query = _quotaRepository.Table
                .Where(x => x.OrganizationId == org 
                            && x.SalesPersonId != _currentUser.UserId
                            && x.ProductId == request.ProductId );
          
            var result = await query
                .OrderBy(x => x.QuotaDate)
                .ToListAsync(cancellationToken: cancellationToken);
            var orderedResult = new List<Entities.Quota>();
            foreach (var quotas in result
                .GroupBy(x=>x.SalesPersonId))
            {
                var sum1 = quotas.Sum(x => x.AvailableQuantity);
                var sum2 = quotas.Sum(x => x.InitialQuantity);
                var sum3 = quotas.Sum(x => x.ReservedQuantity);
                var quota = quotas.FirstOrDefault();
                if (quota == null || sum1< request.Quantity) continue;
                quota.AvailableQuantity = sum1;
                quota.InitialQuantity = sum2;
                //quota.ReservedQuantity = sum3;
                orderedResult.Add(quota);
            }

            var r = orderedResult
                .ToList()
                .AsQueryable()
                .Paged(request.DataGridQuery.Skip/ request.DataGridQuery.Take + 1, request.DataGridQuery.Take);
            //.ToArray();
            return new SyncPagedResult<QuotaDto> { Count =  orderedResult.Count, Result = _mapper.Map<List<QuotaDto>>(r.Distinct().ToList())};
        }

        public async Task<Guid> Handle(GetCurrentUser request, CancellationToken cancellationToken)
        {
            var id =  _currentUser.UserId;
            return id;
        }

        public async Task<IEnumerable<QuotaDto>> Handle(GetQuotasByProductQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return default;
            var userId = request.SalesPeronId ?? _currentUser.UserId;
            var query = await _quotaRepository.Table
                .Where(x => x.OrganizationId == org 
                            && x.SalesPersonId == userId
                            && x.ProductId == request.ProductId)
                .ToListAsync(cancellationToken);
            var result = new List<QuotaDto>();
            foreach (var items in query.GroupBy(x=>x.ProductId))
            {
                var item = _mapper.Map<QuotaDto>(items.FirstOrDefault());
                item.AvailableQuantity = items.Sum(x => x.AvailableQuantity);
                item.OldAvailableQuantity = items.Sum(x => x.AvailableQuantity);
                result.Add( item);
            }
            return new List<QuotaDto>(result);
        }

        public async Task<IEnumerable<QuotaDto>> Handle(GetDetailedQuotaQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new  List<QuotaDto>();
            var currentUser = await _commandBus.SendAsync(new GetUserQuery {Id = _currentUser.UserId, IncludeRoles = true},cancellationToken);

            var query = _quotaRepository.Table.Where(x => x.OrganizationId == org && x.ProductId == request.ProductId && x.SalesPersonId == request.SalesPersonId);
            if (currentUser.UserRoles.Any(x => x.Role.Name == "Admin"))
            {
                
            }
            else if (currentUser.UserRoles.Any(x=>x.Role.Name=="Supervisor"))
            {
                var ids =await _commandBus.SendAsync(new GetSalesPersonsBySupervisorQuery {Id = currentUser.Id},cancellationToken);
                var enumerable = ids as Guid[] ?? ids.ToArray();
                var predicateBuilder = PredicateBuilder.New<Entities.Quota>();
                foreach (var id in enumerable)
                {
                    predicateBuilder.Or(x => x.SalesPersonId == id);
                }
                query = query.Where(predicateBuilder);
            }
            else if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesPerson"))
                query = query.Where(x => x.SalesPersonId == currentUser.Id);

            var r  =await  query
                .Where(x => x.OrganizationId == org
                            && x.ProductId == request.ProductId ).ToListAsync(cancellationToken);
            var result = new List<QuotaDto>();
           /* foreach (var items in r.GroupBy(x=>x.CustomerId))
            {
                var item = _mapper.Map<QuotaDto>(items.FirstOrDefault());
                item.AvailableQuantity = items.Sum(x => x.AvailableQuantity);
                item.InitialQuantity = items.Sum(x => x.InitialQuantity);
                item.ReservedQuantity = items.Sum(x => x.ReservedQuantity);
              //  if(item.InitialQuantity - item.ReservedQuantity == 0) continue;
                result.Add( item);
            }*/

          
            return new List<QuotaDto>(result);
        }

        public async Task<int> Handle(GetQuotasByProductQueryV2 request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return default;
             var query = await _quotaRepository.Table
                .Where(x => x.OrganizationId == org 
                            && x.ProductId == request.ProductId )
                .SumAsync(x=>x.AvailableQuantity, cancellationToken);


            return query;
        }

        public async Task<int> Handle(GetQuotasForSalesPersonByProductIdQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return 0;
            // gets the global qnt of the current product
            var globalQnt = await _quotaRepository.Table
                .Where(x => x.OrganizationId == org 
                            && x.SalesPersonId == request.SalesPersonId 
                            && x.ProductId == request.ProductId)
                .SumAsync(x => x.AvailableQuantity, cancellationToken: cancellationToken);

            if (globalQnt == 0)
                return 0;
            
            //obtain the sum of consumed quota by customer.
            var sumOfConsumedQuantities = await (from qt in _quotaRepository.Table
                        .Include(x => x.QuotaTransactions)
                    from trans in qt.QuotaTransactions
                    where qt.SalesPersonId == request.SalesPersonId
                          && trans.CustomerId == request.CustomerId
                          && qt.ProductId == request.ProductId
                    select trans.Quantity)
                .SumAsync(cancellationToken: cancellationToken);
            //gets  initial quantities allocated to the customer.
            var sumOfInitialQuantities = await _quotaInitRepository.Table
                .Where(x => x.OrganizationId == org
                            && x.QuotaId == request.SalesPersonId
                            && x.ProductId == request.ProductId
                            && x.CustomerId == request.CustomerId)
                .SumAsync(x => x.Quantity, cancellationToken: cancellationToken);
           //  if the global qnt is less than the consumed qnt , return global qnt     
            if (sumOfInitialQuantities - sumOfConsumedQuantities > globalQnt)
                return globalQnt;
            //otherwise , return the difference between the initial quantities allocated to the customer and the consumed quantities.
            // Q_difference = Q_allocated - Q_consumed
            return sumOfInitialQuantities - sumOfConsumedQuantities;
        }

        public async Task<int> Handle(GetQuotasByProductQueryV3 request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return default;
            var userId = request.SalesPeronId ?? _currentUser.UserId;
            var query = await _quotaRepository.Table
                .Where(x => x.OrganizationId == org 
                            && x.SalesPersonId == userId
                            && x.ProductId == request.ProductId)
                .SumAsync(x=>x.AvailableQuantity, cancellationToken);
            return query;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.StockState
{
    public class GetStockStatePagedQuery : ICommand<SyncPagedResult<StockStateDtoV1>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }

    
    public class GetStockStatePagedQueryHandler : ICommandHandler<GetStockStatePagedQuery, SyncPagedResult<StockStateDtoV1>>
    {
        private readonly IRepository<Entities.StockState, Guid> _stockZoneRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public GetStockStatePagedQueryHandler(IRepository<Entities.StockState, Guid> stockZoneRepository, 
            ICurrentOrganization currentOrganization 
            , IMapper mapper)
        {
            _stockZoneRepository = stockZoneRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<StockStateDtoV1>> Handle(GetStockStatePagedQuery request, CancellationToken cancellationToken)
        
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<StockStateDtoV1>();
            var query = _stockZoneRepository.Table.Include(x=>x.ZoneType).AsQueryable();
            if (request.SyncDataGridQuery.Where != null)
            {
                foreach (var wherePredicate in request.SyncDataGridQuery.Where[0].Predicates)
                {
                    query = wherePredicate.Field == "zoneType" ? 
                        query.Where(x => x.ZoneType.Name.Contains(wherePredicate.Value.ToString())) :
                        query.Where($"{wherePredicate.Field}.Contains(@0)", wherePredicate.Value);
                }
               
            }
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query
                .OrderByDescending(x => x.CreatedDateTime)
                .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<StockStateDtoV1>>(result);

            return new SyncPagedResult<StockStateDtoV1> {Result = data, Count = total};
        }
    }
}
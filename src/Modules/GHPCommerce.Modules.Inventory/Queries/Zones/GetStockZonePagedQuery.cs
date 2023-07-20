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
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Zones
{
    public class GetStockZonePagedQuery: ICommand<SyncPagedResult<StockZoneDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }

    public class GetStockZonePagedQueryHandler : ICommandHandler<GetStockZonePagedQuery, SyncPagedResult<StockZoneDto>>
    {
        private readonly IRepository<StockZone, Guid> _stockZoneRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public GetStockZonePagedQueryHandler(IRepository<StockZone, Guid> stockZoneRepository , 
            ICurrentOrganization currentOrganization, 
            IMapper mapper)
        {
            _stockZoneRepository = stockZoneRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<StockZoneDto>> Handle(GetStockZonePagedQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<StockZoneDto>();
            var query = _stockZoneRepository.Table.Where(x => x.OrganizationId == org);
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
            var data = _mapper.Map<List<StockZoneDto>>(result);

            return new SyncPagedResult<StockZoneDto>
                {Result = data, Count = total};
            
        }
    }
}
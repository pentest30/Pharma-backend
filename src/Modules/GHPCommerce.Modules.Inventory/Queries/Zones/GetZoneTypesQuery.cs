using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Inventory.DTOs;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Zones
{
    public class GetZoneTypesQuery : ICommand<SyncPagedResult<ZoneTypeDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }

    public class GetZoneTypesQueryHandler : ICommandHandler<GetZoneTypesQuery, SyncPagedResult<ZoneTypeDto>>
    {
        private readonly IRepository<ZoneType, Guid> _zoneRepository;
        private readonly IMapper _mapper;

        public GetZoneTypesQueryHandler(IRepository<ZoneType, Guid> zoneRepository, IMapper mapper)
        {
            _zoneRepository = zoneRepository;
            _mapper = mapper;
        }

        public async Task<SyncPagedResult<ZoneTypeDto>> Handle(GetZoneTypesQuery request, CancellationToken cancellationToken)
        {
            var query = _zoneRepository.Table;
            if (request.SyncDataGridQuery.Where != null)
            {
                query = request.SyncDataGridQuery.Where[0].Predicates.Aggregate(query,
                    (current, wherePredicate) =>
                        current.Where($"{wherePredicate.Field}.Contains(@0)", wherePredicate.Value));
            }

            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query
                .OrderByDescending(x => x.CreatedDateTime)
                .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<ZoneTypeDto>>(result);

            return new SyncPagedResult<ZoneTypeDto>
                {Result = data, Count = total};
        }
    }
}

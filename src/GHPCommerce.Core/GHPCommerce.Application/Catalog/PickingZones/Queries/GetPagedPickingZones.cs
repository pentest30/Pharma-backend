using AutoMapper;
using GHPCommerce.Application.Catalog.PickingZones.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Application.Catalog.PickingZones.Queries
{
    public class GetPagedPickingZones : ICommand<SyncPagedResult<PickingZoneDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
    public class GetPagedPickingZonesHandler : ICommandHandler<GetPagedPickingZones, SyncPagedResult<PickingZoneDto>>
    {
        private readonly IRepository<PickingZone, Guid> _taxRepository;
        private readonly IMapper _mapper;

        public GetPagedPickingZonesHandler(IRepository<PickingZone, Guid> PickingZoneRepository, IMapper mapper)
        {
            _taxRepository = PickingZoneRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<PickingZoneDto>> Handle(GetPagedPickingZones request, CancellationToken cancellationToken)
        {
            var query = _taxRepository.Table.Include(x=> x.ZoneGroup).AsNoTracking()
                .DynamicWhereQuery(request.GridQuery);
            var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
            var result = await query
                .Page(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<List<PickingZoneDto>>(result);
            return new SyncPagedResult<PickingZoneDto> { Result = data, Count = total };
        }
    }
}

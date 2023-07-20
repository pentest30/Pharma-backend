using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.PickingZones.DTOs;
using GHPCommerce.Core.Shared.Contracts.PickingZone.DTOs;
using GHPCommerce.Core.Shared.Contracts.PickingZone.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.PickingZones.Queries
{
    public class PickingZoneQueriesHandler :
        ICommandHandler<GetAllPickingZonesQuery, IEnumerable<PickingZoneDto>>,
        ICommandHandler<GetPickingZonesListQuery, PagingResult<PickingZoneDto>>,
                ICommandHandler<GetPickingZoneByTypeQuery, PickingZoneDtoV1>,
        ICommandHandler<GetPickingZoneByNameQuery, PickingZoneDtoV1>

    {
        private readonly IRepository<PickingZone, Guid> _pickingZoneRepository;
        private readonly IMapper _mapper;

        public PickingZoneQueriesHandler(IRepository<PickingZone, Guid> pickingZoneRepository, IMapper mapper)
        {
            _pickingZoneRepository = pickingZoneRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PickingZoneDto>> Handle(GetAllPickingZonesQuery request,
            CancellationToken cancellationToken)
        {
                                                                                                                                    var query = await _pickingZoneRepository
                .Table
                .Include(x => x.ZoneGroup)
                .ToListAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<PickingZoneDto>>(query);
        }

        public async Task<PagingResult<PickingZoneDto>> Handle(GetPickingZonesListQuery request, CancellationToken cancellationToken)
        {
            var total = await _pickingZoneRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _pickingZoneRepository
                .Table
                                .Include(x => x.ZoneGroup)
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<IEnumerable<PickingZoneDto>>(query);
            return new PagingResult<PickingZoneDto> { Data = data, Total = total };
        }

        public async Task<PickingZoneDtoV1> Handle(GetPickingZoneByTypeQuery request, CancellationToken cancellationToken)
        {
            var query = await _pickingZoneRepository
                                .Table
                                .Include(x => x.ZoneGroup)
                                .FirstOrDefaultAsync(x => x.ZoneType == request.ZoneType, cancellationToken: cancellationToken);
            return _mapper.Map<PickingZoneDtoV1>(query);
        }
        public async Task<PickingZoneDtoV1> Handle(GetPickingZoneByNameQuery request, CancellationToken cancellationToken)
        {
            var query = await _pickingZoneRepository
                                .Table
                                .Include(x => x.ZoneGroup)
                                .FirstOrDefaultAsync(x => x.Name.ToLower() == request.ZoneName.ToLower(), cancellationToken: cancellationToken);
            return _mapper.Map<PickingZoneDtoV1>(query);
        }
    }
}

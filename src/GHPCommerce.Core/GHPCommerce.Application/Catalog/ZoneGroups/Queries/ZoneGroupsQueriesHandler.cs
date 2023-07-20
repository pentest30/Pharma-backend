using AutoMapper;
using GHPCommerce.Application.Catalog.ZoneGroups.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.ZoneGroup.Queries;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace GHPCommerce.Application.Catalog.ZoneGroups.Queries
{
    class ZoneGroupsQueriesHandler :
        ICommandHandler<GetPagedZoneGroupQuery, SyncPagedResult<ZoneGroupDto>>,
        ICommandHandler<GetAllZoneGroupQuery, IEnumerable<ZoneGroupDto>>,
        ICommandHandler<GetGroupZoneByIdQuery, GHPCommerce.Core.Shared.Contracts.ZoneGroup.DTOs.ZoneGroupDto>

    {
        private readonly IRepository<ZoneGroup, Guid> _groupZoneRepository;
        private readonly IMapper _mapper;

        public ZoneGroupsQueriesHandler(IRepository<ZoneGroup, Guid> groupZoneRepository, IMapper mapper)
        {
            _groupZoneRepository = groupZoneRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<ZoneGroupDto>> Handle(GetPagedZoneGroupQuery request, CancellationToken cancellationToken)
        {
            var query = _groupZoneRepository.Table;
            var result = await query
                 .OrderByDescending(x => x.Name)
                 .ToListAsync(cancellationToken: cancellationToken);
            var r = result
               .Skip(request.DataGridQuery.Skip / request.DataGridQuery.Take)
               .Take(request.DataGridQuery.Take);
            return new SyncPagedResult<ZoneGroupDto> {
                Count = result.Count(),
                Result = _mapper.Map<List<ZoneGroupDto>>(r)
            };

        }

        public async Task<IEnumerable<ZoneGroupDto>> Handle(GetAllZoneGroupQuery request, CancellationToken cancellationToken)
        {
            var query = await _groupZoneRepository
                          .Table
                          .OrderBy(x => x.Name)
                          .ToListAsync(cancellationToken);
            var result = _mapper.Map<List<ZoneGroupDto>>(query);

            return result;
        }

        public async Task<Core.Shared.Contracts.ZoneGroup.DTOs.ZoneGroupDto> Handle(GetGroupZoneByIdQuery request, CancellationToken cancellationToken)
        {
            var zone = await _groupZoneRepository
                .Table
                .Where(x => x.Id ==request.Id)
                .FirstOrDefaultAsync(cancellationToken);
            var result = _mapper.Map<Core.Shared.Contracts.ZoneGroup.DTOs.ZoneGroupDto>(zone);

            return result;        
        }
    }
}

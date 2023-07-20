using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.DTOs.Zone;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Modules.Inventory.Queries.Zones
{
    public class GetAllZoneQuery : ICommand<IEnumerable<ZoneDto>>
    {
    }
    public class GetAllZoneQueryHandler : ICommandHandler<GetAllZoneQuery, IEnumerable<ZoneDto>>
    {
        private readonly IRepository<StockZone, Guid> _stockZoneRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;


        public GetAllZoneQueryHandler(
            IRepository<StockZone, Guid> stockZoneRepository,
           IMapper mapper,
           ICurrentOrganization currentOrganization)
        {
            _stockZoneRepository = stockZoneRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
        }

        public async Task<IEnumerable<ZoneDto>> Handle(GetAllZoneQuery request, CancellationToken cancellationToken)
        {
            var query = await _stockZoneRepository
                       .Table
                       .ToListAsync(cancellationToken);
            var result = _mapper.Map<List<ZoneDto>>(query);

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.DTOs.Invent;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Invent
{
    public class GetInventByZoneQuery : ICommand<IEnumerable<InventDtoV1>>
    {
        public Guid ZoneId { get; set; }
        public Guid StockStateId { get; set; }
    }
    public class GetInventByZoneQueryHandler : ICommandHandler<GetInventByZoneQuery, IEnumerable<InventDtoV1>>
    {
        private readonly IRepository<Entities.Invent, Guid> _inventRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;

        public GetInventByZoneQueryHandler(IRepository<Entities.Invent, Guid> inventRepository, 
            IMapper mapper,
            ICurrentOrganization currentOrganization)
        {
            _inventRepository = inventRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
        }
        public async Task<IEnumerable<InventDtoV1>> Handle(GetInventByZoneQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var result =  await _inventRepository.Table
                .Where(x => x.ZoneId == request.ZoneId 
                            && x.OrganizationId == orgId
                            && x.PhysicalQuantity - x.PhysicalReservedQuantity>0
                            &&x.StockStateId == request.StockStateId)
                .ToListAsync(cancellationToken: cancellationToken);
            return _mapper.Map<List<InventDtoV1>>(result);
        }
    }
}
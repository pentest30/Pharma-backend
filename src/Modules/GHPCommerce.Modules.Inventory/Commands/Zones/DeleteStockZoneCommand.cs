using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Commands.Zones
{
    public class DeleteStockZoneCommand : ICommand
    {
        public Guid Id { get; set; }
    }
    public class DeleteStockZoneCommandHandler : ICommandHandler<DeleteZoneTypeCommand>
    {
        private readonly IRepository<StockZone, Guid> _zoneTypeRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;


        public DeleteStockZoneCommandHandler(IRepository<StockZone, Guid> zoneTypeRepository,
            IMapper mapper,
            ICurrentOrganization currentOrganization)
        {
            _zoneTypeRepository = zoneTypeRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
        }
        public async Task<Unit> Handle(DeleteZoneTypeCommand request, CancellationToken cancellationToken)
        {
            // var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var existingZoneType =
                await _zoneTypeRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken: cancellationToken);
            if (existingZoneType == null) throw new NotFoundException("Zone type with id " + request.Id + " was not found");
            _zoneTypeRepository.Delete(existingZoneType);
            await _zoneTypeRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }

    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Commands.Zones
{
    public class UpdateZoneTypeCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    public class UpdateZoneTypeCommandsHandler : ICommandHandler<UpdateZoneTypeCommand, ValidationResult>
    {
        private readonly IRepository<ZoneType, Guid> _zoneTypeRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;


        public UpdateZoneTypeCommandsHandler(IRepository<ZoneType, Guid> zoneTypeRepository,
            IMapper mapper,
            ICurrentOrganization currentOrganization)
        {
            _zoneTypeRepository = zoneTypeRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
        }
        public async Task<ValidationResult> Handle(UpdateZoneTypeCommand request, CancellationToken cancellationToken)
        {

            var existingZoneType =
                await _zoneTypeRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken: cancellationToken);
            if (existingZoneType ==null) throw new NotFoundException("Zone type with id " + request.Id + " was not found");
            existingZoneType.Name = request.Name;
            _zoneTypeRepository.Update(existingZoneType);
            await _zoneTypeRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }
       
    }
}

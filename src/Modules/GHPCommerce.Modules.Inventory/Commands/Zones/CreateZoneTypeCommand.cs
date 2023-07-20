using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Commands.Zones
{
    public class CreateZoneTypeCommand :ICommand<ValidationResult>
    {
        public string Name { get; set; }
    }
    public  class ZonesTypesCommandsHandler : ICommandHandler<CreateZoneTypeCommand, ValidationResult>
    {
        private readonly IRepository<ZoneType, Guid> _zoneTypeRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;


        public ZonesTypesCommandsHandler(IRepository<ZoneType, Guid> zoneTypeRepository, 
            IMapper mapper, 
            ICurrentOrganization currentOrganization)
        {
            _zoneTypeRepository = zoneTypeRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
        }
        public async Task<ValidationResult> Handle(CreateZoneTypeCommand request, CancellationToken cancellationToken)
        {
           // var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var validationErrors = new ValidationResult();
            await ValidateName(request.Name, cancellationToken,validationErrors );
            if (!validationErrors.IsValid) return validationErrors;
            var zoneType =new ZoneType();
            zoneType.Name = request.Name;
            _zoneTypeRepository.Add(zoneType);
            await _zoneTypeRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }
        private async Task ValidateName( string name, CancellationToken cancellationToken, ValidationResult validationErrors)
        {
            var existingForm =
                await _zoneTypeRepository.Table.AnyAsync(x => x.Name == name ,
                    cancellationToken: cancellationToken);
            if (existingForm)
                validationErrors.Errors.Add(new ValidationFailure("Code",
                    "There is a zone type with the same  name, please change the  name "));
        }
    }
}

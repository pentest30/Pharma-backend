using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Commands.Zones
{
    public class UpdateStockZoneCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public Guid ZoneTypeId { get; set; }
        public string Name { get; set; }
        public  EntityStatus EntityStatus { get; set; }
    }

    public class UpdateStockZoneCommandHandler : ICommandHandler<UpdateStockZoneCommand, ValidationResult>
    {
        private readonly IRepository<StockZone, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;

        public UpdateStockZoneCommandHandler(IRepository<StockZone, Guid> repository,
            ICurrentOrganization currentOrganization)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
        }

        public async Task<ValidationResult> Handle(UpdateStockZoneCommand request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if(org == default) throw new InvalidOperationException("Invalid operation");
            var existingZoneType =
                await _repository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken: cancellationToken);
            if (existingZoneType == null) throw new NotFoundException("Zone type with id " + request.Id + " was not found");
            existingZoneType.Name = request.Name;
            existingZoneType.ZoneState = request.EntityStatus;
            existingZoneType.ZoneTypeId = request.ZoneTypeId;
            _repository.Update(existingZoneType);
            await _repository.UnitOfWork.SaveChangesAsync();
            return default;
        }
    }
}

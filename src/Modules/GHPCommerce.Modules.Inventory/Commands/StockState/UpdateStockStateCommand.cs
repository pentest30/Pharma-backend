using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Modules.Inventory.Commands.StockState
{
    public class UpdateStockStateCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public EntityStatus StockStatus { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ZoneTypeId { get; set; }

    }
    public class UpdateStockStateCommandHandler : ICommandHandler<UpdateStockStateCommand, ValidationResult>
    {
        private readonly IRepository<Entities.StockState, Guid> _stockStateRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;


        public UpdateStockStateCommandHandler(
           IRepository<Entities.StockState, Guid> stockStateRepository,
           IMapper mapper,
           ICurrentOrganization currentOrganization)
        {
            _stockStateRepository = stockStateRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
        }
        public async Task<ValidationResult> Handle(UpdateStockStateCommand request, CancellationToken cancellationToken)
        {
            var existingZoneType =
                await _stockStateRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (existingZoneType == null) throw new NotFoundException("Stock State with id " + request.Id + " was not found");
            existingZoneType = _mapper.Map<Entities.StockState>(request);
            _stockStateRepository.Update(existingZoneType);
            await _stockStateRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }
    }
}

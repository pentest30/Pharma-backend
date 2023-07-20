using AutoMapper;
using FluentValidation.Results;
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
    public class CreateStockStateCommand : ICommand<ValidationResult>
    {
        public Guid ZoneTypeId { get; set; }
        public string Name { get; set; }
        public EntityStatus StockStatus { get; set; }
        public Guid OrganizationId { get; set; }
    }

    public class CreateStockStateCommandHandler : ICommandHandler<CreateStockStateCommand, ValidationResult>
    {
        private readonly IRepository<Entities.StockState, Guid> _stockStateRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;


        public CreateStockStateCommandHandler(
            IRepository<Entities.StockState, Guid> stockStateRepository,
           IMapper mapper,
           ICurrentOrganization currentOrganization)
        {
            _stockStateRepository = stockStateRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
        }
        public async Task<ValidationResult> Handle(CreateStockStateCommand request, CancellationToken cancellationToken)
        {
            // var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var validationErrors = new ValidationResult();
            await ValidateName(request.Name, cancellationToken, validationErrors);
            if (!validationErrors.IsValid) return validationErrors;
            var stockState = _mapper.Map<Entities.StockState>(request);
            _stockStateRepository.Add(stockState);
            await _stockStateRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }
        private async Task ValidateName(string name, CancellationToken cancellationToken, ValidationResult validationErrors)
        {
            var existingForm =
                await _stockStateRepository.Table.AnyAsync(x => x.Name == name,
                    cancellationToken: cancellationToken);
            if (existingForm)
                validationErrors.Errors.Add(new ValidationFailure("Code",
                    "There is a state with the same  name, please change the  name "));
        }
    }
}

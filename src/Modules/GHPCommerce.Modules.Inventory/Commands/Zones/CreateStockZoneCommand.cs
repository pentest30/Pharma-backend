using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Commands.Zones
{
    public class CreateStockZoneCommand : ICommand<ValidationResult>
    {
        public Guid ZoneTypeId { get; set; }
        public string Name { get; set; }
    }

    public class CreateStockZoneCommandValidator : AbstractValidator<CreateStockZoneCommand>
    {
        public CreateStockZoneCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.ZoneTypeId)
                .Must(x=>x!=Guid.Empty);

        }
    }
    public class CreateStockZoneCommandHandler : ICommandHandler<CreateStockZoneCommand, ValidationResult>
    {
        private readonly IRepository<StockZone, Guid> _repository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _organization;

        public CreateStockZoneCommandHandler(IRepository<StockZone, Guid> repository, IMapper mapper, ICurrentOrganization organization)
        {
            _repository = repository;
            _mapper = mapper;
            _organization = organization;
        }

        public async Task<ValidationResult> Handle(CreateStockZoneCommand request, CancellationToken cancellationToken)
        {
            var org = await _organization.GetCurrentOrganizationIdAsync();
            var validator = new CreateStockZoneCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            var existingForm = await _repository.Table.AnyAsync(x => x.Name == request.Name, cancellationToken);
            if (existingForm)
            {
                validationErrors.Errors.Add(new ValidationFailure("Name",
                    "There is a Zone with the same  name, please change the  name "));
            }

            if (!validationErrors.IsValid)
                return validationErrors;
            var stock = new StockZone();
            stock.Name = request.Name;
            stock.ZoneTypeId = request.ZoneTypeId;
            stock.OrganizationId = org.Value;
            stock.ZoneState = EntityStatus.Active;
            _repository.Add(stock);
            await _repository.UnitOfWork.SaveChangesAsync();
            return validationErrors;
        }
    }
}

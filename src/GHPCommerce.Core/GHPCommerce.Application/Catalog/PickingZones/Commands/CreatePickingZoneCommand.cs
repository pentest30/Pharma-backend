using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.PickingZones.Commands
{
    public class CreatePickingZoneCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public ZoneType ZoneType { get; set; }

    }
    public class CreatePickingZoneCommandValidator : AbstractValidator<CreatePickingZoneCommand>
    {
        public CreatePickingZoneCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}

using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.PickingZones.Commands
{
    public class UpdatePickingZoneCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public int Order { get; set; }
        public ZoneType ZoneType { get; set; }


    }
    public class UpdatePickingZoneCommandValidator : AbstractValidator<UpdatePickingZoneCommand>
    {
        public UpdatePickingZoneCommandValidator()
        {
            RuleFor(v => v.Name)
                .MaximumLength(200).
                NotEmpty();
        }
    }
}

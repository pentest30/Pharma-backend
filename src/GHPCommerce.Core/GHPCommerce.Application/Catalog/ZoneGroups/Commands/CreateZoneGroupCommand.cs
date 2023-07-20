using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Application.Catalog.ZoneGroups.Commands
{
    public class CreateZoneGroupCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int order { get; set; }
        public string printer { get; set; }

    }
    public class CreateZoneGroupCommandValidator : AbstractValidator<CreateZoneGroupCommand>
    {
        public CreateZoneGroupCommandValidator()
        {

        }

    }

}
